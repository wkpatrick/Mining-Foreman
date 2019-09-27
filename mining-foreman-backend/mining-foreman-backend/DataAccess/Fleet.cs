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

        public static Models.Network.MiningFleet SelectFleet(int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                var fleet =  conn.QueryFirst<Models.Network.MiningFleet>(@" SELECT * FROM MiningFleets WHERE MiningFleetKey = @MiningFleetKey", new {MiningFleetKey = miningFleetKey});
                fleet.FleetMembers =
                    conn.Query<Models.Network.MiningFleetMember>(@"
                    SELECT DISTINCT mfm.UserKey, u.CharacterName, u.CharacterId, mfm.MiningFleetMemberKey, mfm.MiningFleetKey FROM MiningFleetMembers mfm
                    JOIN Users u ON mfm.userkey = u.UserKey
                    WHERE MiningFleetKey = @MiningFleetKey", new{MiningFleetKey = miningFleetKey}).ToList();
                foreach (var member in fleet.FleetMembers) {
                    if (fleet.IsActive) {
                        member.MemberMiningLedger = MiningLedger.SelectActiveFleetProductionByUser(member.UserKey, miningFleetKey);
                    }
                    else {
                        member.MemberMiningLedger = MiningLedger.SelectFinishedFleetProductionByUser(member.UserKey, miningFleetKey);
                    }
                }
                fleet.FleetBoss = SelectFleetBoss(miningFleetKey);
                return fleet;
            }
        }

        public static Models.Network.MiningFleetMember SelectFleetBoss(int miningFleetKey) {
            using (var conn = ConnectionFactory()) {
                var fleetBossKey = conn.QuerySingle<int>(
                    @"SELECT FleetBossKey FROM MiningFleets WHERE MiningFleetKey = @MiningFleetKey",
                    new {MiningFleetKey = miningFleetKey});
                return SelectFleetMember(miningFleetKey, fleetBossKey);
            }
        }
        
        public static Models.Network.MiningFleetMember SelectFleetMember(int fleetKey, int userKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                //Ideally I would like to not have to pull in the whole fleet here to do this.
                var fleet =  conn.QueryFirst<Models.MiningFleet>(@" SELECT * FROM MiningFleets WHERE MiningFleetKey = @MiningFleetKey", new {MiningFleetKey = fleetKey});
                
                //TODO: Check to see if the user is in the fleet when adding stuff
                var member = conn.QuerySingle<Models.Network.MiningFleetMember>(@"
                SELECT mfm.UserKey, u.CharacterName, u.CharacterId, mfm.MiningFleetMemberKey, mfm.MiningFleetKey
                FROM MiningFleetMembers mfm
                JOIN Users u ON mfm.userkey = u.UserKey
                WHERE mfm.MiningFleetKey = @MiningFleetKey AND mfm.UserKey = @UserKey", new{MiningFleetKey = fleetKey, UserKey = userKey});
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
                conn.Execute(@"UPDATE MiningFleets SET IsActive = FALSE, EndTime = now() at time zone 'utc' WHERE MiningFleetKey = @MiningFleetKey",
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

        public static int SelectActiveFleetByUserKey(int userKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.QuerySingleOrDefault<int>(@"
                SELECT mf.miningfleetkey FROM MiningFleetMembers mfm
                JOIN MiningFleets mf ON mfm.MiningFleetKey = mf.MiningFleetKey
                WHERE mfm.UserKey = @UserKey AND mf.IsActive = true ",
                    new {UserKey = userKey});
            }
        }
    }
}