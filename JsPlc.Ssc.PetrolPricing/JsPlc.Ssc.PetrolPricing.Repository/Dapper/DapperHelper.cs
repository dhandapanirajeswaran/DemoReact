using Dapper;
using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Core.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace JsPlc.Ssc.PetrolPricing.Repository.Dapper
{
    public static class DapperHelper
    {
        public static string DatabaseConnectionString = "";

        public static void Execute(this DbContext context, string sprocName, object parameters, bool disableDapperLog = false)
        {
            if (!disableDapperLog)
                LogDapperCall(sprocName, parameters);

            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            var conn = CreateConnection();
            using (conn)
            {
                conn.Execute(sprocName, parameters, null, null, CommandType.StoredProcedure);
            }
        }

        public static T QueryFirst<T>(this DbContext context, string sprocName, object parameters, bool disableDapperLog = false) where T : class
        {
            if (!disableDapperLog)
                LogDapperCall(sprocName, parameters);

            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            var conn = CreateConnection();
            using (conn)
            {
                return conn.QueryFirst<T>(sprocName, parameters, null, null, CommandType.StoredProcedure);
            }
        }

        public static int QueryScalar(this DbContext context, string sprocName, object parameters, bool disableDapperLog = false)
        {
            if (!disableDapperLog)
                LogDapperCall(sprocName, parameters);

            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            var conn = CreateConnection();
            using (conn)
            {
                return conn.ExecuteScalar<int>(sprocName, parameters, null, null, CommandType.StoredProcedure);
            }
        }

        public static List<T> QueryList<T>(this DbContext context, string sprocName, object parameters, bool disableDapperLog = false) where T : class
        {
            if (!disableDapperLog)
                LogDapperCall(sprocName, parameters);

            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            var conn = CreateConnection();
            using (conn)
            {
                return conn.Query<T>(sprocName, parameters, null, false, null, CommandType.StoredProcedure).ToList();
            }
        }

        public static T QueryMultiple<T>(this DbContext context, string sprocName, object parameters, Action<T, SqlMapper.GridReader> filler, bool disableDapperLog = false) where T : class, new()
        {
            if (!disableDapperLog)
                LogDapperCall(sprocName, parameters);

            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            T result = new T();

            var conn = CreateConnection();
            using (conn)
            {
                var multiReader = conn.QueryMultiple(sprocName, parameters, null, null, CommandType.StoredProcedure);
                filler(result, multiReader);
                return result;
            }
        }

        #region private methods

        private static SqlConnection CreateConnection()
        {
            if (String.IsNullOrEmpty(DatabaseConnectionString))
                throw new Exception("Dapper - DatabaseConnectionString is empty !");
            return new SqlConnection(DatabaseConnectionString);
        }

        private static void LogDapperCall(string sprocName, object parameters)
        {
            if (CoreSettings.RepositorySettings.Dapper.LogDapperCalls)
            {
                var msg = new StringBuilder();
                msg.AppendFormat("Dapper Sproc: {0}", sprocName);

                var exception = "";
                var parameterDictionary = new Dictionary<string, string>();

                if (parameters != null)
                {
                    try
                    {
                        var properties = parameters.GetType().GetProperties();
                        foreach (var prop in properties)
                        {
                            object value = prop.GetValue(parameters, null);
                            parameterDictionary.Add(prop.Name, value == null ? "" : value.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        msg.Append(" - ***EXCEPTION***");
                        exception = ex.ToString();
                    }
                }

                DiagnosticLog.StartDebug(msg.ToString(), parameterDictionary, exception);
            }
        }

        #endregion private methods
    }
}