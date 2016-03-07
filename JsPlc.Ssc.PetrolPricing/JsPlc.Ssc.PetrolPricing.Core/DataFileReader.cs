using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
	public class DataFileReader : IDataFileReader
	{
		public DataTable GetQuarterlyData(string filePathAndName, string excelFileSheetName)
		{
			var connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1'", filePathAndName);

			using (var adapter = new OleDbDataAdapter(String.Format("SELECT * FROM [{0}$]",
				excelFileSheetName), connectionString))
			{
				using (var ds = new DataSet())
				{
					adapter.Fill(ds, "x");
					return ds.Tables[0].Copy();
				}
			}
		}
	}
}
