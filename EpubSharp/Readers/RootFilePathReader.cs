using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using EpubSharp.Utils;

namespace EpubSharp.Readers
{
    internal static class RootFilePathReader
    {
        public static string GetRootFilePath(ZipArchive epubArchive)
        {
            const string epubContainerFilePath = "META-INF/container.xml";
            ZipArchiveEntry containerFileEntry = epubArchive.GetEntryIgnoringSlashDirection(epubContainerFilePath);
            if (containerFileEntry == null)
                throw new Exception(string.Format("EPUB parsing error: {0} file not found in archive.", epubContainerFilePath));
            XmlDocument containerDocument;
            using (Stream containerStream = containerFileEntry.Open())
                containerDocument = XmlUtils.LoadDocument(containerStream);
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(containerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("cns", "urn:oasis:names:tc:opendocument:xmlns:container");
            XmlNode rootFileNode = containerDocument.DocumentElement.SelectSingleNode("/cns:container/cns:rootfiles/cns:rootfile", xmlNamespaceManager);
            return rootFileNode.Attributes["full-path"].Value;
        }
    }
}
