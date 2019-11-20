using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace mining_foreman_backend.DataAccess {
    public class User : DataAccess {
        public static Models.Database.User SelectUser(int characterId) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.QueryFirstOrDefault<Models.Database.User>(
                    @"SELECT * FROM Users WHERE characterid = @CharacterId LIMIT 1",
                    new {CharacterId = characterId});
            }
        }

        public static Models.Database.User SelectUserByAPIToken(string apiToken) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                var user = conn.QuerySingle<Models.Database.User>(
                    @"SELECT * FROM Users WHERE APIToken = @APIToken",
                    new {APIToken = apiToken});
                user.ActiveFleetKey = Fleet.SelectActiveFleetByUserKey(user.UserKey);
                return user;
            }
        }

        public static long InsertUser(Models.Database.User user) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Execute(
                    @"INSERT INTO Users (CharacterId, CharacterName, AccessToken, RefreshToken, RefreshTokenExpiresUtc, APIToken) VALUES(@CharacterId, @CharacterName, @AccessToken, @RefreshToken, @RefreshTokenExpiresUTC, @APIToken)",
                    user);
            }
        }

        public static void UpdateUser(Models.Database.User user) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(
                    @"UPDATE Users SET CharacterId = @CharacterId, CharacterName = @CharacterName, AccessToken = @AccessToken, RefreshToken = @RefreshToken, RefreshTokenExpiresUTC = @RefreshTokenExpiresUTC, APIToken = @APIToken WHERE UserKey = @UserKey", user);
            }
        }

        public static List<Models.Database.User> SelectAllUsers() {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Query<Models.Database.User>(@"SELECT * FROM Users").ToList();
            }
        }

        public static void UpdateCharacter(Models.Database.User user) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                foreach (var item in user.CharacterMining.Model) {
                    var dbItem = conn.QueryFirstOrDefault<Models.MiningLedger>(
                        @"SELECT * FROM MiningLedger WHERE Date = @Date AND SolarSystemId = @SolarSystemId AND TypeId = @TypeId LIMIT 1",
                        item);
                    if (dbItem == null) {
                        conn.Execute(
                            @"INSERT INTO MiningLedger (UserKey, Date, Quantity, SolarSystemId, TypeId) VALUES(@UserKey, @Date, @Quantity, @SolarSystemId, @TypeId)",
                            new {
                                UserKey = user.UserKey, Date = item.Date, Quantity = item.Quantity,
                                SolarSystemId = item.SolarSystemId, TypeId = item.TypeId
                            });
                    }
                    else {
                        conn.Execute(
                            @"UPDATE MiningLedger SET Quantity = @Quantity WHERE Date = @Date AND SolarSystemId = @SolarSystemId AND TypeId = @TypeId AND UserKey = @UserKey",
                            new {
                                UserKey = user.UserKey, Date = item.Date, Quantity = item.Quantity,
                                SolarSystemId = item.SolarSystemId, TypeId = item.TypeId
                            });
                    }
                }
            }
        }

        public static int SelectUserKeyByCharacterId(int characterId) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.QuerySingle<int>(@"SELECT UserKey FROM Users WHERE CharacterId = @CharacterId",
                    new {CharacterId = characterId});
            }
        }

        public static bool SelectIsUserValidForFleet(int userKey, int fleetKey) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                var activeFleets = conn.Query<Models.MiningFleet>(@"
                SELECT mf.* FROM MiningFleets mf
                JOIN MiningFleetMembers mfm ON mf.MiningFleetKey = mfm.MiningFleetKey
                WHERE mf.IsActive = true AND mfm.IsActive = true AND UserKey = @UserKey",
                    new {UserKey = userKey}).ToList();

                if (activeFleets.Count > 0) {
                    return false;
                }

                return true;
            }
        }
    }
}