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

            
            if (!File.Exists(filePathAndName))
            {
                throw new ExcelParseFileException("excel file:" + filePathAndName + ". cannot exist. Contact support team." + System.Environment.NewLine, null);
			
            }

			using (var adapter = new OleDbDataAdapter(String.Format("SELECT * FROM [{0}$]",
				excelFileSheetName), connectionString))
			{
				using (var ds = new DataSet())
				{
					try
					{
						adapter.Fill(ds, "x");
					}
					catch (OleDbException ex)
					{
						if (ex.Message.Contains("Make sure that it does not include invalid characters or punctuation and that it is not too long."))
						{
							var message = string.Format("Invalid Sheet 1 name. Expected name: {0}. Fix the issue and try again.", excelFileSheetName);

							throw new ExcelParseFileException(message, ex);
						}
						else
						{
							throw;
						}
					}
					catch (Exception ex)
					{
                        throw new ExcelParseFileException("Unable to read excel file. Contact support team." + System.Environment.NewLine + ex.Message, ex);
					}
					return ds.Tables[0].Copy();
				}
			}
		}
	}
}
