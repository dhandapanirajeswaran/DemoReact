using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ReactApi.Model;
using ReactApi.Repository.Query;

namespace ReactApi.Repository
{
    public class Database : IDatabase
    {
        readonly string _connectionString;
        public Database(string connectionString)
        {
            this._connectionString = connectionString;
        }



        public async Task<T> QueryAsync<T>(IQuery<T> query)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                await connection.OpenAsync();
                return await query.ExecuteAsync(connection);

            }
        }
    }
}
