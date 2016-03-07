using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
	public static class Extensions
	{
		public static List<DataRow> ToDataRowsList(this DataTable dataTable)
		{
			var rowCount = dataTable.Rows.Count;
			DataRow[] retval = { };
			if (rowCount <= 0) return retval.ToList();

			var rowsArr = new DataRow[rowCount];
			dataTable.Rows.CopyTo(rowsArr, 0);
			retval = rowsArr;
			return retval.ToList();
		}
	}
}
