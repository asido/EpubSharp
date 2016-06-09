using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using EpubSharp.Format;
using EpubSharp.Utils;

namespace EpubSharp.Readers
{
    internal static class OcfReader
    {
        public static OcfDocument Read(ZipArchive epubArchive)
        {
            const string epubContainerFilePath = "META-INF/container.xml";
            var containerFileEntry = epubArchive.GetEntryIgnoringSlashDirection(epubContainerFilePath);
            if (containerFileEntry == null)
            {
                throw new Exception($"EPUB parsing error: {epubContainerFilePath} file not found in archive.");
            }

            XmlDocument containerDocument;
            using (var containerStream = containerFileEntry.Open())
            {
                containerDocument = XmlUtils.LoadDocument(containerStream);
            }
            var xmlNamespaceManager = new XmlNamespaceManager(containerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("cns", "urn:oasis:names:tc:opendocument:xmlns:container");
            var rootFileNode = containerDocument.DocumentElement.SelectSingleNode("/cns:container/cns:rootfiles/cns:rootfile", xmlNamespaceManager);
            var ocf = new OcfDocument
            {
                RootFile = rootFileNode.Attributes["full-path"].Value
            };
            return ocf;
        }
    }
}
