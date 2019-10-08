namespace mining_foreman_backend.Models.Database {
    public class PendingMiningFleetLedger {
        public int PendingMiningLedgerKey { get; set; }
        public int MiningFleetKey { get; set; }
        public int MemberKey { get; set; }
    }
}