using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace mining_foreman_backend.DataAccess {
    public class Fleet : DataAccess {
        //TODO: Fleets need to be associated with Corps/Alliances and thus need to filter based on the logged on user's corp/Alliance
        public static List<Models.MiningFleet> SelectActiveFleets() {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Query<Models.MiningFleet>(@" SELECT * FROM MiningFleets WHERE IsActive = TRUE ORDER BY MiningFleetKey DESC").ToList();
            }
        }

        public static Models.MiningFleet SelectFleet(int fleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                var fleet =  conn.QueryFirst<Models.MiningFleet>(@" SELECT * FROM MiningFleets WHERE MiningFleetKey = @MiningFleetKey", new {MiningFleetKey = fleetKey});
                fleet.FleetMembers =
                    conn.Query<Models.MiningFleetMember>(
                        @"SELECT DISTINCT mfm.UserKey, mfm.MiningFleetMemberKey, MiningFleetKey FROM MiningFleetMembers mfm WHERE MiningFleetKey = @MiningFleetKey", new{MiningFleetKey = fleetKey}).ToList();
                foreach (var member in fleet.FleetMembers) {
                    if (fleet.IsActive) {
                        member.MemberMiningLedger = MiningLedger.SelectActiveFleetProductionByUser(member.UserKey, fleetKey);
                    }
                    else {
                        member.MemberMiningLedger = MiningLedger.SelectFinishedFleetProductionByUser(member.UserKey, fleetKey);
                    }
                }
                return fleet;
            }
        }

        //TODO: Selects a single fleet member's contributions.
        public static Models.MiningFleetMember SelectFleetMember(int fleetKey, int userKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                //Ideally I would like to not have to pull in the whole fleet here to 
                var fleet =  conn.QueryFirst<Models.MiningFleet>(@" SELECT * FROM MiningFleets WHERE MiningFleetKey = @MiningFleetKey", new {MiningFleetKey = fleetKey});
                var member = conn.QuerySingle<Models.MiningFleetMember>(
                    @"SELECT mfm.UserKey, mfm.MiningFleetMemberKey, MiningFleetKey FROM MiningFleetMembers mfm WHERE MiningFleetKey = @MiningFleetKey AND UserKey = @UserKey", new{MiningFleetKey = fleetKey, UserKey = userKey});
                if (fleet.IsActive) {
                    member.MemberMiningLedger = MiningLedger.SelectActiveFleetProductionByUser(member.UserKey, fleetKey);
                }
                else {
                    member.MemberMiningLedger = MiningLedger.SelectFinishedFleetProductionByUser(member.UserKey, fleetKey);
                }
                return member;
            }
        }

        public static int InsertMiningFleet(Models.MiningFleet fleet) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.QuerySingle<int>(
                    @"INSERT INTO MiningFleets (FleetBossKey, StartTime, EndTime, IsActive) 
                            VALUES (@FleetBossKey, @StartTime, @EndTime, @IsActive) RETURNING MiningFleetKey;",
                    new {
                        FleetBossKey = fleet.FleetBossKey, StartTime = fleet.StartTime, EndTime = fleet.EndTime,
                        IsActive = fleet.IsActive
                    });
            }
        }

        public static void EndMiningFleet(int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(@"UPDATE MiningFleets SET IsActive = FALSE, EndTime = now() WHERE MiningFleetKey = @MiningFleetKey",
                    new {MiningFleetKey = miningFleetKey});
            }
        }

        public static void InsertMiningFleetMember(int userKey, int fleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(
                    @"INSERT INTO MiningFleetMembers (MiningFleetKey, UserKey) VALUES (@MiningFleetKey, @UserKey)",
                    new {MiningFleetKey = fleetKey, UserKey = userKey});
            }
        }
    }
}