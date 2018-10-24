using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ReactApi.Model;
using ReactApi.Repository;
using ReactApi.Repository.Query;

namespace ReactApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IDatabase _database;

        public ProductService(IDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            IEnumerable<Product> products = await _database.QueryAsync(new GetProduct());
            return products.Select(x => new Product
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName
            }).ToList();
        }
    }
}
