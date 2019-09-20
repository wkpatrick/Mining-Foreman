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
    }
}