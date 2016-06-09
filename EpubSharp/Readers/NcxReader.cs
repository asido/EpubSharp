using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using EpubSharp.Format;
using EpubSharp.Schema.Navigation;

namespace EpubSharp.Readers
{
    internal static class NcxReader
    {
        public static NcxDocument Read(XmlDocument xml)
        {
            NcxDocument result = new NcxDocument();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xml.NameTable);
            xmlNamespaceManager.AddNamespace("ncx", "http://www.daisy.org/z3986/2005/ncx/");
            XmlNode headNode = xml.DocumentElement.SelectSingleNode("ncx:head", xmlNamespaceManager);
            if (headNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain head element");
            result.Metadata = ReadNavigationHead(headNode);
            XmlNode docTitleNode = xml.DocumentElement.SelectSingleNode("ncx:docTitle", xmlNamespaceManager);
            if (docTitleNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain docTitle element");
            result.DocTitle = ReadNavigationDocTitle(docTitleNode);

            var authors = new List<string>();
            foreach (XmlNode docAuthorNode in xml.DocumentElement.SelectNodes("ncx:docAuthor", xmlNamespaceManager))
            {
                authors.AddRange(ReadNavigationDocAuthor(docAuthorNode));
            }
            result.DocAuthors = authors.AsReadOnly();

            XmlNode navMapNode = xml.DocumentElement.SelectSingleNode("ncx:navMap", xmlNamespaceManager);
            if (navMapNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain navMap element");
            result.NavigationPoints = ReadNavigationMap(navMapNode);
            XmlNode pageListNode = xml.DocumentElement.SelectSingleNode("ncx:pageList", xmlNamespaceManager);
            if (pageListNode != null)
            {
                result.PageList = ReadNavigationPageList(pageListNode);
            }
            result.NavLists = (from XmlNode navigationListNode in xml.DocumentElement.SelectNodes("ncx:navList", xmlNamespaceManager)
                               select ReadNavigationList(navigationListNode)).ToList().AsReadOnly();
            return result;
        }

        private static IReadOnlyCollection<EpubNcxMetadata> ReadNavigationHead(XmlNode headNode)
        {
            var result = new List<EpubNcxMetadata>();
            foreach (XmlNode metaNode in headNode.ChildNodes)
                if (string.Compare(metaNode.LocalName, "meta", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    EpubNcxMetadata meta = new EpubNcxMetadata();
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

        private static IReadOnlyCollection<EpubNcxNavigationPoint> ReadNavigationMap(XmlNode navigationMapNode)
        {
            return (from XmlNode navigationPointNode in navigationMapNode.ChildNodes
                    where string.Compare(navigationPointNode.LocalName, "navPoint", StringComparison.OrdinalIgnoreCase) == 0
                    select ReadNavigationPoint(navigationPointNode)).ToList().AsReadOnly();
        }

        private static EpubNcxNavigationPoint ReadNavigationPoint(XmlNode navigationPointNode)
        {
            EpubNcxNavigationPoint result = new EpubNcxNavigationPoint();
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
            var navLabels = new List<string>();
            result.NavigationPoints = new List<EpubNcxNavigationPoint>();
            foreach (XmlNode navigationPointChildNode in navigationPointNode.ChildNodes)
            {
                switch (navigationPointChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        navLabels.Add(ReadNavigationLabel(navigationPointChildNode));
                        break;
                    case "content":
                        result.ContentSource = ReadNavigationContent(navigationPointChildNode);
                        break;
                    case "navpoint":
                        EpubNcxNavigationPoint childNavigationPoint = ReadNavigationPoint(navigationPointChildNode);
                        result.NavigationPoints.Add(childNavigationPoint);
                        break;
                }
            }
            result.NavigationLabels = navLabels.AsReadOnly();
            if (!result.NavigationLabels.Any())
                throw new Exception($"EPUB parsing error: navigation point {result.Id} should contain at least one navigation label");
            if (result.ContentSource == null)
                throw new Exception($"EPUB parsing error: navigation point {result.Id} should contain content");
            return result;
        }

        private static string ReadNavigationLabel(XmlNode navigationLabelNode)
        {
            var navigationLabelTextNode = navigationLabelNode.ChildNodes.OfType<XmlNode>()
                .FirstOrDefault(node => string.Compare(node.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0);
            if (navigationLabelTextNode == null)
                throw new Exception("Incorrect EPUB navigation label: label text element is missing");
            return navigationLabelTextNode.InnerText;
        }

        private static string ReadNavigationContent(XmlNode navigationContentNode)
        {
            return navigationContentNode.Attributes["src"].Value;
        }

        private static IReadOnlyCollection<EpubNavigationPageTarget> ReadNavigationPageList(XmlNode navigationPageListNode)
        {
            return (from XmlNode pageTargetNode in navigationPageListNode.ChildNodes
                    where string.Compare(pageTargetNode.LocalName, "pageTarget", StringComparison.OrdinalIgnoreCase) == 0
                    select ReadNavigationPageTarget(pageTargetNode)).ToList().AsReadOnly();
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
            {
                throw new Exception("Incorrect EPUB navigation page target: page target type is missing");
            }

            var navLabels = new List<string>();
            foreach (XmlNode navigationPageTargetChildNode in navigationPageTargetNode.ChildNodes)
            {
                switch (navigationPageTargetChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        navLabels.Add(ReadNavigationLabel(navigationPageTargetChildNode));
                        break;
                    case "content":
                        result.ContentSource = ReadNavigationContent(navigationPageTargetChildNode);
                        break;
                }
            }
            result.NavigationLabels = navLabels.AsReadOnly();
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
            var navLabels = new List<string>();
            foreach (XmlNode navigationListChildNode in navigationListNode.ChildNodes)
            {
                switch (navigationListChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        navLabels.Add(ReadNavigationLabel(navigationListChildNode));
                        break;
                    case "navTarget":
                        var navigationTarget = ReadNavigationTarget(navigationListChildNode);
                        result.NavigationTargets.Add(navigationTarget);
                        break;
                }
            }
            result.NavigationLabels = navLabels.AsReadOnly();
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
            var navLabels = new List<string>();
            foreach (XmlNode navigationTargetChildNode in navigationTargetNode.ChildNodes)
            {
                switch (navigationTargetChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        navLabels.Add(ReadNavigationLabel(navigationTargetChildNode));
                        break;
                    case "content":
                        var content = ReadNavigationContent(navigationTargetChildNode);
                        result.ContentSource = content;
                        break;
                }
            }
            result.NavigationLabels = navLabels.AsReadOnly();
            if (!result.NavigationLabels.Any())
                throw new Exception("Incorrect EPUB navigation target: at least one navLabel element is required");
            return result;
        }
    }
}
