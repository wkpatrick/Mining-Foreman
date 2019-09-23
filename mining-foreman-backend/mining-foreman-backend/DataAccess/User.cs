using System.Collections.Generic;
using System.Linq;
using Dapper;
using EVEStandard.Models;

namespace mining_foreman_backend.DataAccess {
    public class User : DataAccess {
        public static Models.User SelectUser(int characterId) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.QueryFirstOrDefault<Models.User>(
                    @"SELECT * FROM Users WHERE characterid = @CharacterId LIMIT 1",
                    new {CharacterId = characterId});
            }
        }

        public static Models.User SelectUserByAPIToken(string apiToken) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.QuerySingle<Models.User>(
                    @"SELECT * FROM Users WHERE APIToken = @APIToken",
                    new {APIToken = apiToken});
            }
        }

        public static long InsertUser(Models.User user) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Execute(
                    @"INSERT INTO Users (CharacterId, AccessToken, RefreshToken, RefreshTokenExpiresUtc) VALUES(@CharacterId, @AccessToken, @RefreshToken, @RefreshTokenExpiresUTC)",
                    user);
            }
        }

        public static void UpdateUser(Models.User user) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(
                    @"UPDATE Users SET CharacterId = @CharacterId, AccessToken = @AccessToken, RefreshToken = @RefreshToken, RefreshTokenExpiresUTC = @RefreshTokenExpiresUTC, APIToken = @APIToken", user);
            }
        }

        public static List<Models.User> SelectAllUsers() {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Query<Models.User>(@"SELECT * FROM Users").ToList();
            }
        }

        public static void UpdateCharacter(Models.User user) {
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
    }
}