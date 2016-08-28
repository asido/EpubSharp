using System;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class OcfReader
    {
        private static readonly XNamespace OcfNamespace = "urn:oasis:names:tc:opendocument:xmlns:container";

        private static class OcfElements
        {
            public static readonly XName RootFile = OcfNamespace + "rootfile";
        }

        public static OcfDocument Read(XmlDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));

            var xmlNamespaceManager = new XmlNamespaceManager(xml.NameTable);
            xmlNamespaceManager.AddNamespace("cns", "urn:oasis:names:tc:opendocument:xmlns:container");
            var rootFileNode = xml.DocumentElement.SelectSingleNode("/cns:container/cns:rootfiles/cns:rootfile", xmlNamespaceManager);
            var ocf = new OcfDocument
            {
                RootFile = rootFileNode.Attributes["full-path"].Value
            };
            return ocf;
        }

        public static OcfDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            var element = xml.Root.Descendants(OcfElements.RootFile).FirstOrDefault();
            var rootFile = element?.Attribute("full-path")?.Value;
            if (string.IsNullOrWhiteSpace(rootFile))
            {
                throw new EpubParseException("Malformed OCF.");
            }
            return new OcfDocument { RootFile = rootFile };
        }
    }
}
