using System;
using System.IO.Compression;
using System.Xml;

namespace EpubSharp.Format.Readers
{
    internal static class OcfReader
    {
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
    }
}
