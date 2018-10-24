using MySql.Data.MySqlClient;
using ReactApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ReactApi.MySql
{
    public class MySqlConnector
    {
        public List<Inventory> GetInventoryData()
        {
            List<Inventory> list = new List<Inventory>();
            string MyConnection2 = "datasource=localhost;port=3306;username=root;password=Oct@12345678";
            string Query = @"select 
                        Store,
                        SKU,
                        Quantity
                        from
                        demo.inventory ";
            MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
            MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {
                list.Add(new Inventory()
                {
                    Store = (int)MyReader2["Store"],
                    SKU = MyReader2["SKU"].ToString(),
                    Quantity = MyReader2["Quantity"].ToString(),

                });
            }
            MyConn2.Close();
            return list;
        }

        public List<Sales> GetSalesData()
        {
            List<Sales> list = new List<Sales>();
            string MyConnection2 = "datasource=localhost;port=3306;username=root;password=Oct@12345678";
            string Query = @"select 
                            Store_Number,
                            Store_Name,
                            Transaction_Date,
                            SKU,
                            Quantity,
                            Unit_Price
                            from
                            demo.transaction_header th
                            inner
                            join
                             demo.line_item lt on th.Header_Number = lt.Header_Number ";

            MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
            MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {
                list.Add(new Sales()
                {
                    Store_Number = (int)MyReader2["Store_Number"],
                    Store_Name = MyReader2["Store_Name"].ToString(),
                    Transaction_Date = MyReader2["Transaction_Date"].ToString(),
                    SKU = MyReader2["SKU"].ToString(),
                    Quantity = (float)MyReader2["Quantity"],
                    Price = (float)MyReader2["Unit_Price"]
                });
            }
            MyConn2.Close();
            return list;
        }

        public long GetSkuQuantityLessThan()
        {
            string MyConnection2 = "datasource=localhost;port=3306;username=root;password=Oct@12345678";
            string Query = @"select Count(Distinct(SKU)) as LessQuality  from demo.Inventory where Quantity< 100";

            MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
            MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            MyReader2 = MyCommand2.ExecuteReader();
            long LessQuality=0;
            while (MyReader2.Read())
            {
                LessQuality=(long)MyReader2["LessQuality"];
            }
          
            MyConn2.Close();
            return LessQuality;
        }
        public long GetSkuQuantityMoreThan()
        {
            string MyConnection2 = "datasource=localhost;port=3306;username=root;password=Oct@12345678";
            string Query = @"select Count(Distinct(SKU)) as MoreQuality from demo.Inventory where Quantity > 0";

            MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);
            MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
            MySqlDataReader MyReader2;
            MyConn2.Open();
            long MoreQuality = 0;
            MyReader2 = MyCommand2.ExecuteReader();

            while (MyReader2.Read())
            {
                MoreQuality = (long)MyReader2["MoreQuality"];
            }
            MyConn2.Close();
            return MoreQuality;
        }
    }
}



