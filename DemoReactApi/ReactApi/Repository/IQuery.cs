using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ReactApi.Repository
{
    public interface IQuery<T>
    {
        Task<T> ExecuteAsync(IDbConnection db);
    }
}
