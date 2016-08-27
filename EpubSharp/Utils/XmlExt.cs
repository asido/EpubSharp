using System.IO;
using System.Xml;

namespace EpubSharp
{
    internal static class XmlExt
    {
        public static XmlDocument LoadDocument(Stream stream)
        {
            var xmlReaderSettings = new XmlReaderSettings
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Ignore
            };
            using (var reader = XmlReader.Create(stream, xmlReaderSettings))
            {
                var doc = new XmlDocument();
                doc.Load(reader);
                return doc;
            }
        }
    }
}
