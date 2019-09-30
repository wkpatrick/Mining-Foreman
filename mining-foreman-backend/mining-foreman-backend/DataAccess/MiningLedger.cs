using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace mining_foreman_backend.DataAccess {
    public class MiningLedger : DataAccess {
        public static List<Models.MiningLedger> SelectMiningLedgerByUser(int userKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Query<Models.MiningLedger>(@"SELECT * FROM MiningLedger WHERE UserKey = @UserKey",
                    new {UserKey = userKey}).ToList();
            }
        }

        public static List<Models.MiningFleetLedger>
            SelectActiveFleetProductionByUser(int userKey, int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();

                return conn.Query<Models.MiningFleetLedger>(@"
                SELECT  ml.TypeId, SUM(ml.quantity - COALESCE(mfl.quantity, 0)) as Quantity, ml.SolarSystemId FROM MiningLedger ml
                JOIN MiningFleetMembers mfm ON ml.UserKey = mfm.UserKey
                JOIN MiningFleets mf ON mfm.MiningFleetKey = mf.MiningFleetKey
                LEFT OUTER JOIN MiningFleetLedger mfl  ON mfl.FleetKey = mf.MiningFleetKey AND mfl.IsStartingLedger = true
                WHERE mf.MiningFleetKey = @MiningFleetKey AND ml.Date >= mf.StartTime::date AND ml.UserKey = @UserKey
                GROUP BY ml.TypeId, ml.SolarSystemId",
                    new {UserKey = userKey, MiningFleetKey = miningFleetKey}).ToList();
            }
        }

        public static List<Models.MiningFleetLedger> SelectFinishedFleetProductionByUser(int userKey,
            int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Query<Models.MiningFleetLedger>(@"
                SELECT * FROM MiningFleetLedger WHERE FleetKey = @MiningFleetKey AND UserKey = @UserKey AND IsStartingLedger = false",
                    new {MiningFleetKey = miningFleetKey, UserKey = userKey}).ToList();
            }
        }

        //TODO: Sum together the multiple ending fleet ledgers from the user if they join leave multiple times
        public static List<Models.FleetTotal> SelectFleetTotalProduction(int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                var fleet = conn.QueryFirst<Models.MiningFleet>(
                    @" SELECT * FROM MiningFleets WHERE MiningFleetKey = @MiningFleetKey",
                    new {MiningFleetKey = miningFleetKey});
                if (fleet.IsActive) {
                    var totalList = conn.Query<Models.FleetTotal>(@"
                    SELECT  ml.TypeId, SUM(ml.quantity - COALESCE(mfl.quantity, 0)) as Quantity FROM MiningLedger ml
                    JOIN MiningFleetMembers mfm ON ml.UserKey = mfm.UserKey
                    JOIN MiningFleets mf ON mfm.MiningFleetKey = mf.MiningFleetKey
                    LEFT OUTER JOIN MiningFleetLedger mfl  ON mfl.FleetKey = mf.MiningFleetKey AND mfl.IsStartingLedger = true
                    WHERE mf.MiningFleetKey = @MiningFleetKey AND ml.Date >= mf.StartTime::date
                    GROUP BY ml.TypeId",
                        new {MiningFleetKey = miningFleetKey}).ToList();
                    
                    /** My query above might just be good enough to not have to deal with this shit
                    totalList.AddRange(conn.Query<Models.FleetTotal>(@"
                    SELECT TypeId, SUM(Quantity) as Quantity FROM MiningFleetLedger 
                    WHERE FleetKey = @MiningFleetKey AND IsStartingLedger = false
                    GROUP BY TypeId",
                        new {MiningFleetKey = miningFleetKey}).ToList());

                    var sumTotal =
                        from total in totalList
                        group total by total.TypeId
                        into typeGroup
                        select new Models.FleetTotal{
                            TypeId = typeGroup.Key,
                            Quantity = typeGroup.Sum(x => x.Quantity)
                        };
                    return sumTotal.ToList();
                    **/
                    return totalList;
                }

                return conn.Query<Models.FleetTotal>(@"
                    SELECT TypeId, SUM(Quantity) as Quantity FROM MiningFleetLedger 
                    WHERE FleetKey = @MiningFleetKey AND IsStartingLedger = false
                    GROUP BY TypeId",
                    new {MiningFleetKey = miningFleetKey}).ToList();
            }
        }
        
        public static void InsertStartingFleetMiningLedger(int userKey, int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                //We have to calc the UTC date in C# since npgsql doesnt deal with the datatype correctly
                //https://github.com/npgsql/npgsql/issues/972
                conn.Execute(@"
                INSERT INTO MiningFleetLedger (FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, IsStartingLedger, LedgerCount)
                SELECT @MiningFleetKey as FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, true::bool as IsStartingLedger,
                (SELECT COALESCE(MAX(LedgerCount) + 1, 0) FROM MiningFleetLedger WHERE FleetKey = @MiningFleetKey AND UserKey = @UserKey) LedgerCount
                FROM MiningLedger WHERE UserKey = @UserKey AND date = @UTCDate",
                    new {UserKey = userKey, MiningFleetKey = miningFleetKey, UTCDate = DateTime.UtcNow.Date});
            }
        }

        public static void InsertEndingFleetMiningLedger(int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(@"
                INSERT INTO MiningFleetLedger (FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, IsStartingLedger, LedgerCount )
                SELECT mf.MiningFleetKey AS FleetKey, ml.UserKey, ml.Date, (ml.quantity - COALESCE(mfl.quantity, 0)) as Quantity, ml.SolarSystemId, ml.TypeId, FALSE as IsStartingLedger FROM MiningLedger ml
                JOIN MiningFleetMembers mlf ON ml.UserKey = mlf.UserKey
                JOIN MiningFleets mf ON mlf.MiningFleetKey = mf.MiningFleetKey
                LEFT OUTER JOIN MiningFleetLedger mfl  ON mfl.FleetKey = mf.MiningFleetKey 
                AND mfl.IsStartingLedger = true 
                AND mfl.LedgerCount = (SELECT COALESCE(MAX(LedgerCount), 0) FROM MiningFleetLedger WHERE FleetKey = mf.MiningFleetKey AND UserKey = ml.UserKey)
                WHERE mf.MiningFleetKey = @MiningFleetKey AND ml.Date >= mf.StartTime::date",
                    new {MiningFleetKey = miningFleetKey});
            }
        }

        public static void InsertEndingFleetMiningLedger(int miningFleetKey, int userKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(@"
                INSERT INTO MiningFleetLedger (FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, IsStartingLedger, LedgerCount )
                SELECT mf.MiningFleetKey AS FleetKey, ml.UserKey, ml.Date, (ml.quantity - COALESCE(mfl.quantity, 0)) as Quantity, 
                ml.SolarSystemId, ml.TypeId, FALSE as IsStartingLedger, COALESCE(mfl.LedgerCount, 0) as LedgerCount 
                FROM MiningLedger ml
                JOIN MiningFleetMembers mlf ON ml.UserKey = mlf.UserKey
                JOIN MiningFleets mf ON mlf.MiningFleetKey = mf.MiningFleetKey
                LEFT OUTER JOIN MiningFleetLedger mfl  ON mfl.FleetKey = mf.MiningFleetKey 
                AND mfl.IsStartingLedger = true 
                AND mfl.LedgerCount = (SELECT COALESCE(MAX(LedgerCount), 0) FROM MiningFleetLedger WHERE FleetKey = mf.MiningFleetKey AND UserKey = @UserKey)
                WHERE mf.MiningFleetKey = @MiningFleetKey AND ml.Date >= mf.StartTime::date AND ml.UserKey = @UserKey",
                    new {MiningFleetKey = miningFleetKey, UserKey = userKey});
            }
        }
    }
}