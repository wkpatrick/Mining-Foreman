using System;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace mining_foreman_backend.DataAccess {
    public class DataAccess {
        public static Func<DbConnection> ConnectionFactory {
            get { return () => new NpgsqlConnection(Startup.Configuration["ConnectionString"]); }
        }
    }
}