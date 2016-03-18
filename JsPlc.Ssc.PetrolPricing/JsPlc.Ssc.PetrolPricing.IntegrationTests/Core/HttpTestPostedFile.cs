using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.IntegrationTests.Core
{
	public class HttpTestPostedFile : HttpPostedFileBase
	{
		FileInfo _fileInfo;
		byte[] _content;
		
		public HttpTestPostedFile(string filePathAndName)
		{
			_fileInfo = new FileInfo(filePathAndName);
			_content = File.ReadAllBytes(filePathAndName);
		}

		public override string FileName
		{
			get
			{
				return _fileInfo.Name;
			}
		}

		public override int ContentLength
		{
			get
			{
				return _content.Length;
			}
		}

		public override Stream InputStream
		{
			get
			{
				return File.OpenRead(_fileInfo.FullName);
			}
		}

		public override void SaveAs(string filename)
		{
			File.WriteAllBytes(filename, _content);
		}
	}
}
