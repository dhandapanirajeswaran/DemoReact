using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
	public interface IDataFileReader
	{
		DataTable GetQuarterlyData(string filePathAndName, string excelFileSheetName);


	}
}
