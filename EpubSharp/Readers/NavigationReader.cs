using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using EpubSharp.Schema.Navigation;
using EpubSharp.Schema.Opf;
using EpubSharp.Utils;

namespace EpubSharp.Readers
{
    internal static class NavigationReader
    {
        public static EpubNavigation ReadNavigation(ZipArchive epubArchive, string contentDirectoryPath, EpubPackage package)
        {
            EpubNavigation result = new EpubNavigation();
            string tocId = package.Spine.Toc;
            if (String.IsNullOrEmpty(tocId))
                throw new Exception("EPUB parsing error: TOC ID is empty.");
            EpubManifestItem tocManifestItem = package.Manifest.FirstOrDefault(item => string.Compare(item.Id, tocId, StringComparison.OrdinalIgnoreCase) == 0);
            if (tocManifestItem == null)
                throw new Exception($"EPUB parsing error: TOC item {tocId} not found in EPUB manifest.");
            string tocFileEntryPath = ZipPathUtils.Combine(contentDirectoryPath, tocManifestItem.Href);
            ZipArchiveEntry tocFileEntry = epubArchive.GetEntryIgnoringSlashDirection(tocFileEntryPath);
            if (tocFileEntry == null)
                throw new Exception($"EPUB parsing error: TOC file {tocFileEntryPath} not found in archive.");
            if (tocFileEntry.Length > Int32.MaxValue)
                throw new Exception($"EPUB parsing error: TOC file {tocFileEntryPath} is bigger than 2 Gb.");
            XmlDocument containerDocument;
            using (Stream containerStream = tocFileEntry.Open())
                containerDocument = XmlUtils.LoadDocument(containerStream);
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(containerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ncx", "http://www.daisy.org/z3986/2005/ncx/");
            XmlNode headNode = containerDocument.DocumentElement.SelectSingleNode("ncx:head", xmlNamespaceManager);
            if (headNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain head element");
            result.Head = ReadNavigationHead(headNode);
            XmlNode docTitleNode = containerDocument.DocumentElement.SelectSingleNode("ncx:docTitle", xmlNamespaceManager);
            if (docTitleNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain docTitle element");
            result.DocTitle = ReadNavigationDocTitle(docTitleNode);

            var authors = new List<string>();
            foreach (XmlNode docAuthorNode in containerDocument.DocumentElement.SelectNodes("ncx:docAuthor", xmlNamespaceManager))
            {
                authors.AddRange(ReadNavigationDocAuthor(docAuthorNode));
            }
            result.DocAuthors = authors.AsReadOnly();

            XmlNode navMapNode = containerDocument.DocumentElement.SelectSingleNode("ncx:navMap", xmlNamespaceManager);
            if (navMapNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain navMap element");
            result.NavMap = ReadNavigationMap(navMapNode);
            XmlNode pageListNode = containerDocument.DocumentElement.SelectSingleNode("ncx:pageList", xmlNamespaceManager);
            if (pageListNode != null)
            {
                EpubNavigationPageList pageList = ReadNavigationPageList(pageListNode);
                result.PageList = pageList;
            }
            result.NavLists = new List<EpubNavigationList>();
            foreach (XmlNode navigationListNode in containerDocument.DocumentElement.SelectNodes("ncx:navList", xmlNamespaceManager))
            {
                EpubNavigationList navigationList = ReadNavigationList(navigationListNode);
                result.NavLists.Add(navigationList);
            }
            return result;
        }

        private static IReadOnlyCollection<EpubNavigationHeadMeta> ReadNavigationHead(XmlNode headNode)
        {
            var result = new List<EpubNavigationHeadMeta>();
            foreach (XmlNode metaNode in headNode.ChildNodes)
                if (string.Compare(metaNode.LocalName, "meta", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    EpubNavigationHeadMeta meta = new EpubNavigationHeadMeta();
                    foreach (XmlAttribute metaNodeAttribute in metaNode.Attributes)
                    {
                        string attributeValue = metaNodeAttribute.Value;
                        switch (metaNodeAttribute.Name.ToLowerInvariant())
                        {
                            case "name":
                                meta.Name = attributeValue;
                                break;
                            case "content":
                                meta.Content = attributeValue;
                                break;
                            case "scheme":
                                meta.Scheme = attributeValue;
                                break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(meta.Name))
                        throw new Exception("Incorrect EPUB navigation meta: meta name is missing");
                    if (meta.Content == null)
                        throw new Exception("Incorrect EPUB navigation meta: meta content is missing");
                    result.Add(meta);
                }
            return result;
        }

        private static IReadOnlyCollection<string> ReadNavigationDocTitle(XmlNode docTitleNode)
        {
            return (from XmlNode textNode in docTitleNode.ChildNodes
                    where string.Compare(textNode.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0
                    select textNode.InnerText).ToList().AsReadOnly();
        }

        private static IReadOnlyCollection<string> ReadNavigationDocAuthor(XmlNode docAuthorNode)
        {
            return (from XmlNode textNode in docAuthorNode.ChildNodes
                    where string.Compare(textNode.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0
                    select textNode.InnerText).ToList().AsReadOnly();
        }

        private static IReadOnlyCollection<EpubNavigationPoint> ReadNavigationMap(XmlNode navigationMapNode)
        {
            return (from XmlNode navigationPointNode in navigationMapNode.ChildNodes
                    where string.Compare(navigationPointNode.LocalName, "navPoint", StringComparison.OrdinalIgnoreCase) == 0
                    select ReadNavigationPoint(navigationPointNode)).ToList().AsReadOnly();
        }

        private static EpubNavigationPoint ReadNavigationPoint(XmlNode navigationPointNode)
        {
            EpubNavigationPoint result = new EpubNavigationPoint();
            foreach (XmlAttribute navigationPointNodeAttribute in navigationPointNode.Attributes)
            {
                var attributeValue = navigationPointNodeAttribute.Value;
                switch (navigationPointNodeAttribute.Name.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                    case "playOrder":
                        result.PlayOrder = attributeValue;
                        break;
                }
            }
            if (string.IsNullOrWhiteSpace(result.Id))
                throw new Exception("Incorrect EPUB navigation point: point ID is missing");
            result.NavigationLabels = new List<EpubNavigationLabel>();
            result.ChildNavigationPoints = new List<EpubNavigationPoint>();
            foreach (XmlNode navigationPointChildNode in navigationPointNode.ChildNodes)
                switch (navigationPointChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationPointChildNode);
                        result.NavigationLabels.Add(navigationLabel);
                        break;
                    case "content":
                        EpubNavigationContent content = ReadNavigationContent(navigationPointChildNode);
                        result.Content = content;
                        break;
                    case "navpoint":
                        EpubNavigationPoint childNavigationPoint = ReadNavigationPoint(navigationPointChildNode);
                        result.ChildNavigationPoints.Add(childNavigationPoint);
                        break;
                }
            if (!result.NavigationLabels.Any())
                throw new Exception($"EPUB parsing error: navigation point {result.Id} should contain at least one navigation label");
            if (result.Content == null)
                throw new Exception($"EPUB parsing error: navigation point {result.Id} should contain content");
            return result;
        }

        private static EpubNavigationLabel ReadNavigationLabel(XmlNode navigationLabelNode)
        {
            var result = new EpubNavigationLabel();
            var navigationLabelTextNode = navigationLabelNode.ChildNodes.OfType<XmlNode>().
                Where(node => string.Compare(node.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
            if (navigationLabelTextNode == null)
                throw new Exception("Incorrect EPUB navigation label: label text element is missing");
            result.Text = navigationLabelTextNode.InnerText;
            return result;
        }

        private static EpubNavigationContent ReadNavigationContent(XmlNode navigationContentNode)
        {
            var result = new EpubNavigationContent();
            foreach (XmlAttribute navigationContentNodeAttribute in navigationContentNode.Attributes)
            {
                var attributeValue = navigationContentNodeAttribute.Value;
                switch (navigationContentNodeAttribute.Name.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "src":
                        result.Source = attributeValue;
                        break;
                }
            }
            if (string.IsNullOrWhiteSpace(result.Source))
                throw new Exception("Incorrect EPUB navigation content: content source is missing");
            return result;
        }

        private static EpubNavigationPageList ReadNavigationPageList(XmlNode navigationPageListNode)
        {
            var result = new EpubNavigationPageList();
            foreach (XmlNode pageTargetNode in navigationPageListNode.ChildNodes)
                if (string.Compare(pageTargetNode.LocalName, "pageTarget", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    EpubNavigationPageTarget pageTarget = ReadNavigationPageTarget(pageTargetNode);
                    result.Add(pageTarget);
                }
            return result;
        }

        private static EpubNavigationPageTarget ReadNavigationPageTarget(XmlNode navigationPageTargetNode)
        {
            var result = new EpubNavigationPageTarget();
            foreach (XmlAttribute navigationPageTargetNodeAttribute in navigationPageTargetNode.Attributes)
            {
                var attributeValue = navigationPageTargetNodeAttribute.Value;
                switch (navigationPageTargetNodeAttribute.Name.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "value":
                        result.Value = attributeValue;
                        break;
                    case "type":
                        EpubNavigationPageTargetType type;
                        if (!Enum.TryParse(attributeValue, out type))
                            throw new Exception($"Incorrect EPUB navigation page target: {attributeValue} is incorrect value for page target type");
                        result.Type = type;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                    case "playOrder":
                        result.PlayOrder = attributeValue;
                        break;
                }
            }
            if (result.Type == default(EpubNavigationPageTargetType))
                throw new Exception("Incorrect EPUB navigation page target: page target type is missing");
            foreach (XmlNode navigationPageTargetChildNode in navigationPageTargetNode.ChildNodes)
                switch (navigationPageTargetChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationPageTargetChildNode);
                        result.NavigationLabels.Add(navigationLabel);
                        break;
                    case "content":
                        EpubNavigationContent content = ReadNavigationContent(navigationPageTargetChildNode);
                        result.Content = content;
                        break;
                }
            if (!result.NavigationLabels.Any())
                throw new Exception("Incorrect EPUB navigation page target: at least one navLabel element is required");
            return result;
        }

        private static EpubNavigationList ReadNavigationList(XmlNode navigationListNode)
        {
            var result = new EpubNavigationList();
            foreach (XmlAttribute navigationListNodeAttribute in navigationListNode.Attributes)
            {
                var attributeValue = navigationListNodeAttribute.Value;
                switch (navigationListNodeAttribute.Name.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                }
            }
            foreach (XmlNode navigationListChildNode in navigationListNode.ChildNodes)
                switch (navigationListChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        var navigationLabel = ReadNavigationLabel(navigationListChildNode);
                        result.NavigationLabels.Add(navigationLabel);
                        break;
                    case "navTarget":
                        var navigationTarget = ReadNavigationTarget(navigationListChildNode);
                        result.NavigationTargets.Add(navigationTarget);
                        break;
                }
            if (!result.NavigationLabels.Any())
                throw new Exception("Incorrect EPUB navigation page target: at least one navLabel element is required");
            return result;
        }

        private static EpubNavigationTarget ReadNavigationTarget(XmlNode navigationTargetNode)
        {
            var result = new EpubNavigationTarget();
            foreach (XmlAttribute navigationPageTargetNodeAttribute in navigationTargetNode.Attributes)
            {
                var attributeValue = navigationPageTargetNodeAttribute.Value;
                switch (navigationPageTargetNodeAttribute.Name.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "value":
                        result.Value = attributeValue;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                    case "playOrder":
                        result.PlayOrder = attributeValue;
                        break;
                }
            }
            if (string.IsNullOrWhiteSpace(result.Id))
                throw new Exception("Incorrect EPUB navigation target: navigation target ID is missing");
            foreach (XmlNode navigationTargetChildNode in navigationTargetNode.ChildNodes)
                switch (navigationTargetChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        var navigationLabel = ReadNavigationLabel(navigationTargetChildNode);
                        result.NavigationLabels.Add(navigationLabel);
                        break;
                    case "content":
                        var content = ReadNavigationContent(navigationTargetChildNode);
                        result.Content = content;
                        break;
                }
            if (!result.NavigationLabels.Any())
                throw new Exception("Incorrect EPUB navigation target: at least one navLabel element is required");
            return result;
        }
    }
}
