using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
	public enum FileUploadTypes { DailyPriceData = 1, QuarterlySiteData = 2 }

	public enum ImportProcessStatuses
	{

		Uploaded = 1,
		Warning = 2,
		Processing = 5,
		Success = 10,
		Calculating = 11,
		CalcFailed = 12,
		Failed = 15,
		ImportAborted = 16,
		CalcAborted = 17

	}
}
