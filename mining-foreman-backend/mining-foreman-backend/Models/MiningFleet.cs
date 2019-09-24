using System;
using System.Collections.Generic;

namespace mining_foreman_backend.Models {
    public class MiningFleet {
        public int MiningFleetKey { get; set; }
        public int FleetBossKey { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public List<MiningFleetMember> FleetMembers { get; set; }
    }

    public class MiningFleetMember {
        public int MiningFleetMemberKey { get; set; }
        public int MiningFleetKey { get; set; }
        public int UserKey { get; set; }
        public List<MiningFleetLedger> MemberMiningLedger { get; set; }
    }

    public class MiningFleetResponse {
        public MiningFleet FleetInfo { get; set; }
        public MiningFleetMember MemberInfo { get; set; }
        public List<FleetTotal> FleetTotal { get; set; }
    }
}