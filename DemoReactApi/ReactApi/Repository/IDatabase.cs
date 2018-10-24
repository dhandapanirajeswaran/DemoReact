using ReactApi.Model;
using ReactApi.Repository.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ReactApi.Repository
{
    public interface IDatabase
    {
        Task<T> QueryAsync<T>(IQuery<T> query);
    }
}
