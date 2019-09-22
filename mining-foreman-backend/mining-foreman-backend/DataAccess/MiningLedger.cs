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

        public static List<Models.MiningFleetLedger> SelectFleetProductionByUser(int userKey, int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Query<Models.MiningFleetLedger>(@"
                SELECT mfl.MiningFleetLedgerKey, mfl.FleetKey, mfl.UserKey, ml.Date,(ml.quantity - mfl.quantity) as Quantity, ml.SolarSystemId, ml.TypeId, mfl.IsStartingLedger FROM MiningFleetLedger mfl
                JOIN MiningLedger ml ON mfl.userkey = ml.userkey AND mfl.typeid = ml.typeid AND mfl.solarsystemid = ml.solarsystemid AND mfl.date = ml.date
                WHERE mfl.fleetkey = @MiningFleetKey AND mfl.UserKey = @UserKey AND mfl.IsStartingLedger = false",
                    new {MiningFleetKey = miningFleetKey, UserKey = userKey}).ToList();
            }
        }

        public static void InsertStartingFleetMiningLedger(int userKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(@"
                INSERT INTO MiningFleetLedger (FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, IsStartingLedger)
                (SELECT @FleetKey as FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, true as IsStartingLedger 
                FROM miningledger WHERE UserKey = @UserKey AND date = now()::date at time zone 'utc')",
                    new {UserKey = userKey});
            }
        }

        public static void InsertEndingFleetMiningLedger(int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(@"
                INSERT INTO MiningFleetLedger (FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, IsStartingLedger )
                SELECT mfl.FleetKey, mfl.UserKey, ml.Date,(ml.quantity - mfl.quantity) as Quantity, ml.SolarSystemId, ml.TypeId, FALSE as IsStartingLedger FROM MiningFleetLedger mfl
                JOIN MiningLedger ml ON mfl.userkey = ml.userkey AND mfl.typeid = ml.typeid AND mfl.solarsystemid = ml.solarsystemid AND mfl.date = ml.date
                WHERE mfl.fleetkey = @MiningFleetKey", new {MiningFleetKey = miningFleetKey});
            }
        }
    }
}