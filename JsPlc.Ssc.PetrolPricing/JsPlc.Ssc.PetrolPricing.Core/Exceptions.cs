using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
	public class FileUploadException : ApplicationException
	{
		public FileUploadException(string message) : base(message) { }
	}

	
	public class DailyFileNewBatchException : ApplicationException
	{
		public DailyFileNewBatchException(string message, Exception ex) : base(message, ex) { }
	}
	public class ImportQuarterlyRecordsToStagingException : ApplicationException
	{
		public ImportQuarterlyRecordsToStagingException(string message) : base(message) { }
	}
	public class CatalistNumberUpdateException : ApplicationException
	{
		public CatalistNumberUpdateException(string message, Exception ex) : base(message, ex) { }
	}
	public class NewSiteException : ApplicationException
	{
		public NewSiteException(string message, Exception ex) : base(message, ex) { }
	}
	public class UpdateSiteException : ApplicationException
	{
		public UpdateSiteException(string message, Exception ex) : base(message, ex) { }
	}
	public class NewSiteToCompetitorException : ApplicationException
	{
		public NewSiteToCompetitorException(string message, Exception ex) : base(message, ex) { }
	}
	public class ExcelParseFileException : ApplicationException
	{
		public ExcelParseFileException(string message, Exception ex) : base(message, ex) { }
	}
	
}
