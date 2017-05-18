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

        /// <summary>
        /// Execute a Stored Procedure using Dapper (no result set)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sprocName"></param>
        /// <param name="parameters"></param>
        /// <param name="disableDapperLog"></param>
        /// <param name="commandTimeoutInSeconds"></param>
        public static void Execute(this DbContext context, string sprocName, object parameters, bool disableDapperLog = false, int commandTimeoutInSeconds = 30)
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

        /// <summary>
        /// Query a single object from a Stored Procedure using Dapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="sprocName"></param>
        /// <param name="parameters"></param>
        /// <param name="disableDapperLog"></param>
        /// <param name="commandTimeoutInSeconds"></param>
        /// <returns></returns>
        public static T QueryFirst<T>(this DbContext context, string sprocName, object parameters, bool disableDapperLog = false, int commandTimeoutInSeconds = 30) where T : class
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

        /// <summary>
        /// Query a single value (Scalar) from a Stored Procedure using Dapper
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sprocName"></param>
        /// <param name="parameters"></param>
        /// <param name="disableDapperLog"></param>
        /// <param name="commandTimeoutInSeconds"></param>
        /// <returns></returns>
        public static int QueryScalar(this DbContext context, string sprocName, object parameters, bool disableDapperLog = false, int commandTimeoutInSeconds = 30)
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

        /// <summary>
        /// Query a list of object T from a Stored Procedure using Dapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="sprocName"></param>
        /// <param name="parameters"></param>
        /// <param name="disableDapperLog"></param>
        /// <param name="commandTimeoutInSeconds"></param>
        /// <returns></returns>
        public static List<T> QueryList<T>(this DbContext context, string sprocName, object parameters, bool disableDapperLog = false, int commandTimeoutInSeconds = 30 ) where T : class
        {
            if (!disableDapperLog)
                LogDapperCall(sprocName, parameters);

            if (String.IsNullOrWhiteSpace(sprocName))
                throw new ArgumentException("Stored procedure name cannot be empty");

            var conn = CreateConnection();
            using (conn)
            {
                return conn.Query<T>(sprocName, parameters, null, false, commandTimeoutInSeconds , CommandType.StoredProcedure).ToList();
            }
        }

        /// <summary>
        /// Query mutiple different object T from a Stored Procedure using Dapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="sprocName"></param>
        /// <param name="parameters"></param>
        /// <param name="filler">Action to fill the result set</param>
        /// <param name="disableDapperLog"></param>
        /// <returns></returns>
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