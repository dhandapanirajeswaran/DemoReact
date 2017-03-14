using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Repository.Dapper
{
    public static class DapperHelper
    {
        public static void Execute(this DbContext context, string sprocName, DynamicParameters parameters)
        {
            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            using (var conn = new SqlConnection(context.Database.Connection.ConnectionString))
            {
                conn.Execute(sprocName, parameters, null, null, CommandType.StoredProcedure);
            }
        }

        public static T QueryFirst<T>(this DbContext context, string sprocName, DynamicParameters parameters) where T : class
        {
            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            using (var conn = new SqlConnection(context.Database.Connection.ConnectionString))
            {
                return conn.QueryFirst<T>(sprocName, parameters, null, null, CommandType.StoredProcedure);
            }
        }

        public static int QueryScalar(this DbContext context, string sprocName, DynamicParameters parameters)
        {
            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            using (var conn = new SqlConnection(context.Database.Connection.ConnectionString))
            {
                return conn.ExecuteScalar<int>(sprocName, parameters, null, null, CommandType.StoredProcedure);
            }
        }

        public static List<T> QueryList<T>(this DbContext context, string sprocName, object parameters) where T : class
        {
            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            using (var conn = new SqlConnection(context.Database.Connection.ConnectionString))
            {
                return conn.Query<T>(sprocName, parameters, null, false, null, CommandType.StoredProcedure).ToList();
            }
        }
    }
}
