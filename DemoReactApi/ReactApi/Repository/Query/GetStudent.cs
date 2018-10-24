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
    public class GetStudent : DatabaseQuerys<IEnumerable<Student>>
    {
        public override async Task<IEnumerable<Student>> ExecuteAsync(IDbConnection db)
        {
            var sql = $@"select * from student ;";

            return await db.QueryAsync<Student>(sql);
        }
    }
}
