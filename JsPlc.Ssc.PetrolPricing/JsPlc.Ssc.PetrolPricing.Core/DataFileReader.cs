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

            string[] sheetnames = GetExcelSheetNames(connectionString);

		    if (!String.IsNullOrEmpty(excelFileSheetName))
		    {
		       
		        if (sheetnames == null)
		        {
		            throw new ExcelParseFileException("Unable to read excel file. Contact support team.", null);
		        }

		        List<string> FilteredSheetNames = sheetnames.Where(xx => xx.Contains(excelFileSheetName)).ToList();

		        if (FilteredSheetNames.Count == 0)
		        {
		            var message =
		                string.Format("Invalid Sheet name. Expected name start with: {0}. Fix the issue and try again.",
		                    excelFileSheetName);

		            throw new ExcelParseFileException(message, null);

		        }
		    }
		    using (var adapter = new OleDbDataAdapter(String.Format("SELECT * FROM [{0}]",
                sheetnames[0]), connectionString))
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
                            var message = string.Format("Invalid Sheet name. Expected name start with: {0}. Fix the issue and try again.", excelFileSheetName);

							throw new ExcelParseFileException(message, ex);
						}
						else
						{
							throw;
						}
					}
					catch (Exception ex)
					{
                        throw new ExcelParseFileException("Unable to read excel file. Contact support team.", ex);
					}
					return ds.Tables[0].Copy();
				}
			}

		}


        private String[] GetExcelSheetNames(string connString)
        {
            OleDbConnection objConn = null;
            System.Data.DataTable dt = null;

            try
            {
                   
                // Create connection object by using the preceding connection string.
                objConn = new OleDbConnection(connString);
                // Open connection with the database.
                objConn.Open();
                // Get the data table containg the schema guid.
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null)
                {
                    return null;
                }

                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;

                // Add the sheet name to the string array.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[i] = row["TABLE_NAME"].ToString();
                    i++;
                }

                // Loop through all of the sheets if you want too...
                for (int j = 0; j < excelSheets.Length; j++)
                {
                    // Query each excel sheet.
                }

                return excelSheets;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                // Clean up.
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }
	}
}
