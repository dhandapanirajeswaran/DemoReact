using JsPlc.Ssc.PetrolPricing.Models;
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

		public static Site ToSite(this QuarterlyUploadStaging source)
		{
			return new Site
			{
				SiteName = source.SiteName,
				Town = source.Town,
				CatNo = source.CatNo,
				Brand = source.Brand,
				Address = source.Addr,
				Suburb = source.Suburb,
				PostCode = source.PostCode,
				Company = source.Company,
				Ownership = source.Ownership,
				IsSainsburysSite = source.Brand.Equals(Const.SAINSBURYS),
				IsActive = true
			};
		}

		public static int ToHashCode(this Site source)
		{
			return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
				source.SiteName,
				source.Town,
				source.Brand,
				source.Address,
				source.Suburb,
				source.PostCode,
				source.Company,
				source.Ownership).GetHashCode();
		}
	}
}
