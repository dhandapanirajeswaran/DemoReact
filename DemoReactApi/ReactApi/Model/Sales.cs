using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReactApi.Model
{
    public class Sales
    {

        public int Store_Number { get; set; }
        public string Store_Name { get; set; }
        public string Transaction_Date { get; set; }

        public string SKU { get; set; }

        public float   Quantity { get; set; }

        public float Price { get; set; }

    }
}

