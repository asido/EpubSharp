using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

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

        public static IReadOnlyCollection<string> AsStringList(this IEnumerable<XElement> self)
        {
            return self.Select(elem => elem.Value).ToList().AsReadOnly();
        }

        public static IReadOnlyCollection<T> AsObjectList<T>(this IEnumerable<XElement> self, Func<XElement, T> factory)
        {
            return self.Select(factory).ToList().AsReadOnly();
        }
    }
}
