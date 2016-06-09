using System.IO;
using System.Xml;

namespace EpubSharp.Utils
{
    internal static class XmlUtils
    {
        public static XmlDocument LoadDocument(Stream stream)
        {
            var result = new XmlDocument();
            var xmlReaderSettings = new XmlReaderSettings
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Ignore
            };
            using (var xmlReader = XmlReader.Create(stream, xmlReaderSettings))
                result.Load(xmlReader);
            return result;
        }
    }
}
