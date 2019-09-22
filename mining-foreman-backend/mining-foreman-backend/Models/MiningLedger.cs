using System;

namespace mining_foreman_backend.Models {
    public class MiningLedger {
        public int MiningLedgerKey { get; set; } = -1;
        public int UserKey { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public int SolarSystemId { get; set; }
        public int TypeId { get; set; }
    }

    public class MiningFleetLedger {
        public int MiningFleetLedgerKey { get; set; }
        public int FleetKey { get; set; }
        public int UserKey { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public int SolarSystemId { get; set; }
        public int TypeId { get; set; }
        public bool IsStartingLedger { get; set; }
    }
}