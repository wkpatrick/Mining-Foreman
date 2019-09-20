using System.Runtime.InteropServices;
using Dapper;

namespace mining_foreman_backend.DataAccess {
    public class MiningLedger : DataAccess {
        public static Models.MiningLedger SelectMiningLedgerByUser(int userKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return null;
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

        public static void InsertEndingFleetMiningLedget(int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(@"
                INSERT INTO MiningFleetLedger (FleetKey, UserKey, Date, Quantity, SolarSystemId, TypeId, IsStartingLedger )
                SELECT mfl.FleetKey, mfl.UserKey, ml.Date,(ml.quantity - mfl.quantity) as Quantity, ml.SolarSystemId, ml.TypeId, FALSE as IsStartingLedger FROM MiningFleetLedger mfl
                JOIN MiningLedger ml ON mfl.userkey = ml.userkey AND mfl.typeid = ml.typeid AND mfl.solarsystemid = ml.solarsystemid AND mfl.date = ml.date
                WHERE mfl.fleetkey = @MiningFleetKey", new{MiningFleetKey = miningFleetKey});
            }
        }
    }
}