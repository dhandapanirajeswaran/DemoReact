using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReactApi.Model;
using ReactApi.MySql;

namespace ReactApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // IEnumerable<Product> result = await _productService.GetProducts();
            MySqlConnector conn = new MySqlConnector();
            List<Sales> list = conn.GetSalesData();
            return Ok(list);
        }
    }
}