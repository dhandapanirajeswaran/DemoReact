using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using ReactApi.Model;

namespace ReactApi.Repository.Query
{
    public class GetProduct : DatabaseQuerys<IEnumerable<Product>>
    {
        public override async Task<IEnumerable<Product>> ExecuteAsync(IDbConnection db)
        {
            var sql = $@"select * from product ;";

            return await db.QueryAsync<Product>(sql);
        }
    }
}
