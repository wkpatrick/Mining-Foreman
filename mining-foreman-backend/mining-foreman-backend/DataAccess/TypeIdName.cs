using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace mining_foreman_backend.DataAccess {
    public class TypeIdName : DataAccess {
        public static List<Models.Database.TypeIdName> SelectUnnamedTypeIds() {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                return conn.Query<Models.Database.TypeIdName>(@"
                SELECT DISTINCT ml.TypeId, tin.TypeName FROM MiningLedger ml
                LEFT OUTER JOIN TypeIdNames tin ON ml.TypeId = tin.TypeId
                WHERE tin.TypeName IS NULL").ToList();
            }
        }

        public static void InsertTypeIdName(Models.Database.TypeIdName name) {
            using (var conn = ConnectionFactory()) {
                conn.Open();
                conn.Execute(@"
                INSERT INTO TypeIdNames(TypeId, TypeName) VALUES (@TypeId, @TypeName)",
                    new {TypeId = name.TypeId, TypeName = name.TypeName});
            }
        }
    }
}