﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ReactApi.Repository
{
    public abstract class DatabaseQuerys<T> : IQuery<T>
    {
        public abstract Task<T> ExecuteAsync(IDbConnection db);
    }
}
