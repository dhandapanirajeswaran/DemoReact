using System.Data.SqlTypes;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Repository.Helpers
{
    public static class SqlHelper
    {
        public static string ToSqlXml<T>(T data)
        {
            var memoryStream = new MemoryStream();
            var xs = new XmlSerializer(typeof(T));
            var xmlTextWriter = new XmlTextWriter(memoryStream, System.Text.Encoding.Unicode);
            xs.Serialize(xmlTextWriter, data);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            memoryStream.Position = 0;
            var sr = new StreamReader(memoryStream);
            return sr.ReadToEnd();
        }
    }
}