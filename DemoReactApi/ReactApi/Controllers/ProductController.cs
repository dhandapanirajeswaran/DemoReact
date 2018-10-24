using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReactApi.Model;
using ReactApi.MySql;
using ReactApi.Services;

namespace ReactApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private readonly IProductService _productService;


        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // IEnumerable<Product> result = await _productService.GetProducts();
            MySqlConnector conn = new MySqlConnector();
            long lessquantity = conn.GetSkuQuantityLessThan();
            long morequality = conn.GetSkuQuantityMoreThan();

            decimal AvailabilityPercent = ((decimal)(morequality - lessquantity) / morequality) * 100;

            List<string> list = new List<string>() { lessquantity.ToString(), morequality.ToString(),Math.Round(AvailabilityPercent,2).ToString() };
            return Ok(list);
        }
    }
}