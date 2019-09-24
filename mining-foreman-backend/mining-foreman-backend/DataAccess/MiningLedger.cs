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

        public static List<Models.MiningFleetLedger> SelectActiveFleetProductionByUser(int userKey, int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                
                return conn.Query<Models.MiningFleetLedger>(@"
                SELECT COALESCE (mfl.MiningFleetLedgerKey, -1) AS MiningFleetLedgerKey, mf.MiningFleetKey AS FleetKey, ml.UserKey, ml.Date, (ml.quantity - COALESCE(mfl.quantity, 0)) as Quantity, ml.SolarSystemId, ml.TypeId, COALESCE(mfl.IsStartingLedger, false) as IsStartingLedger FROM MiningLedger ml
                JOIN MiningFleetMembers mlf ON ml.UserKey = mlf.UserKey
                JOIN MiningFleets mf ON mlf.MiningFleetKey = mf.MiningFleetKey
                LEFT OUTER JOIN MiningFleetLedger mfl  ON mfl.FleetKey = mf.MiningFleetKey
                WHERE ml.UserKey = @UserKey AND mf.MiningFleetKey = @MiningFleetKey AND ml.Date >= mf.StartTime::date",
                    new {UserKey = userKey, MiningFleetKey = miningFleetKey}).ToList();
            }
        }

        public static List<Models.MiningFleetLedger> SelectFinishedFleetProductionByUser(int userKey, int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Query<Models.MiningFleetLedger>(@"
                SELECT * FROM MiningFleetLedger WHERE FleetKey = @MiningFleetKey AND UserKey = @UserKey AND IsStartingLedger = false",
                    new {MiningFleetKey = miningFleetKey, UserKey = userKey}).ToList();
            }
        }
        
        public static List<Models.FleetTotal> SelectFleetTotalProduction(int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                var fleet =  conn.QueryFirst<Models.MiningFleet>(@" SELECT * FROM MiningFleets WHERE MiningFleetKey = @MiningFleetKey", new {MiningFleetKey = miningFleetKey});
                if (fleet.IsActive) {
                    return conn.Query<Models.FleetTotal>(@"
                    SELECT SUM(ml.quantity - COALESCE(mfl.quantity, 0)) as Quantity, ml.TypeId FROM MiningLedger ml
                    JOIN MiningFleetMembers mlf ON ml.UserKey = mlf.UserKey
                    JOIN MiningFleets mf ON mlf.MiningFleetKey = mf.MiningFleetKey
                    LEFT OUTER JOIN MiningFleetLedger mfl  ON mfl.FleetKey = mf.MiningFleetKey
                    WHERE mf.MiningFleetKey = @MiningFleetKey AND ml.Date = mf.StartTime::date
                    GROUP BY ml.TypeId",
                        new {MiningFleetKey = miningFleetKey}).ToList();
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
                conn.Execute(@"
                INSERT INTO MiningFleetLedger (FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, IsStartingLedger)
                (SELECT @FleetKey as FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, true as IsStartingLedger 
                FROM miningledger WHERE UserKey = @UserKey AND date = now()::date at time zone 'utc')",
                    new {UserKey = userKey, FleetKey = miningFleetKey});
            }
        }

        public static void InsertEndingFleetMiningLedger(int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(@"
                INSERT INTO MiningFleetLedger (FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, IsStartingLedger )
                SELECT mf.MiningFleetKey AS FleetKey, ml.UserKey, ml.Date, (ml.quantity - COALESCE(mfl.quantity, 0)) as Quantity, ml.SolarSystemId, ml.TypeId, FALSE as IsStartingLedger FROM MiningLedger ml
                JOIN MiningFleetMembers mlf ON ml.UserKey = mlf.UserKey
                JOIN MiningFleets mf ON mlf.MiningFleetKey = mf.MiningFleetKey
                LEFT OUTER JOIN MiningFleetLedger mfl  ON mfl.FleetKey = mf.MiningFleetKey
                WHERE mf.MiningFleetKey = @MiningFleetKey AND ml.Date >= mf.StartTime::date", new {MiningFleetKey = miningFleetKey});
            }
        }
    }
}