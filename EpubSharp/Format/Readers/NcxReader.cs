using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class NcxReader
    {
        private static readonly XNamespace NcxNamespace = "http://www.daisy.org/z3986/2005/ncx/";

        private static class NcxElements
        {
            public static readonly XName Head = NcxNamespace + "head";
            public static readonly XName Meta = NcxNamespace + "meta";
            public static readonly XName DocTitle = NcxNamespace + "docTitle";
            public static readonly XName DocAuthor = NcxNamespace + "docAuthor";
            public static readonly XName Text = NcxNamespace + "text";
            public static readonly XName NavMap = NcxNamespace + "navMap";
            public static readonly XName NavPoint = NcxNamespace + "navPoint";
            public static readonly XName NavList = NcxNamespace + "navList";
            public static readonly XName PageList = NcxNamespace + "pageList";
            public static readonly XName PageTarget = NcxNamespace + "pageTarget";
            public static readonly XName NavLabel = NcxNamespace + "navLabel";
            public static readonly XName NavTarget = NcxNamespace + "navTarget";
            public static readonly XName Content = NcxNamespace + "content";
        }

        public static NcxDocument Read(XmlDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.DocumentElement == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            var ncx = new NcxDocument();
            var xmlNamespaceManager = new XmlNamespaceManager(xml.NameTable);
            xmlNamespaceManager.AddNamespace("ncx", NcxNamespace.NamespaceName);

            var headNode = xml.DocumentElement.SelectSingleNode("ncx:head", xmlNamespaceManager);
            if (headNode == null)
            {
                throw new EpubException("EPUB parsing error: TOC file does not contain head element");
            }
            
            ncx.Metadata = ReadNavigationHead(headNode);
            var docTitleNode = xml.DocumentElement.SelectSingleNode("ncx:docTitle", xmlNamespaceManager);
            if (docTitleNode == null)
            {
                throw new EpubException("EPUB parsing error: TOC file does not contain docTitle element");
            }

            ncx.DocTitle = ReadNavigationDocTitle(docTitleNode).SingleOrDefault();

            var authors = new List<string>();
            foreach (XmlNode docAuthorNode in xml.DocumentElement.SelectNodes("ncx:docAuthor", xmlNamespaceManager))
            {
                authors.AddRange(ReadNavigationDocAuthor(docAuthorNode));
            }
            ncx.DocAuthor = authors.FirstOrDefault();

            var navMapNode = xml.DocumentElement.SelectSingleNode("ncx:navMap", xmlNamespaceManager);
            if (navMapNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain navMap element");
            ncx.NavigationMap = ReadNavigationMap(navMapNode);
            var pageListNode = xml.DocumentElement.SelectSingleNode("ncx:pageList", xmlNamespaceManager);
            if (pageListNode != null)
            {
                ncx.PageList = ReadNavigationPageList(pageListNode);
            }
            ncx.NavigationList = (from XmlNode navigationListNode in xml.DocumentElement.SelectNodes("ncx:navList", xmlNamespaceManager)
                               select ReadNavigationList(navigationListNode)).SingleOrDefault();
            return ncx;
        }

        public static NcxDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            var navList = xml.Root.Element(NcxElements.NavList);
            var ncx = new NcxDocument
            {
                Metadata = xml.Root.Element(NcxElements.Head)?.Elements(NcxElements.Meta).Select(elem => new EpubNcxMetadata
                {
                    Name = (string)elem.Attribute("name"),
                    Content = (string)elem.Attribute("content"),
                    Scheme = (string)elem.Attribute("scheme")
                }).ToList().AsReadOnly(),
                DocTitle = xml.Root.Element(NcxElements.DocTitle)?.Element(NcxElements.Text)?.Value,
                DocAuthor = xml.Root.Element(NcxElements.DocAuthor)?.Element(NcxElements.Text)?.Value,
                NavigationMap = xml.Root.Element(NcxElements.NavMap)?.Elements(NcxElements.NavPoint).Select(ReadNavigationPoint).ToList().AsReadOnly(),
                PageList = xml.Root.Element(NcxElements.PageList)?.Elements(NcxElements.PageTarget).Select(elem => new EpubNcxPageTarget
                {
                    Id = (string)elem.Attribute("id"),
                    Class = (string)elem.Attribute("class"),
                    Value = (int?)elem.Attribute("value"),
                    Type = (EpubNcxPageTargetType?)(elem.Attribute("type") == null ? null : Enum.Parse(typeof(EpubNcxPageTargetType), (string)elem.Attribute("type"), true)),
                    Label = elem.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                    ContentSource = (string)elem.Element(NcxElements.Content)?.Attribute("src")
                }).ToList().AsReadOnly(),
                NavigationList = navList == null ? null : new EpubNcxNavigationList
                {
                    Id = (string)navList.Attribute("id"),
                    Class = (string)navList.Attribute("class"),
                    Label = navList.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                    NavigationTargets = navList.Elements(NcxElements.NavTarget).Select(elem => new EpubNcxNavigationTarget
                    {
                        Id = (string)elem.Attribute("id"),
                        Class = (string)elem.Attribute("class"),
                        Label = navList.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                        PlayOrder = (int?)elem.Attribute("playOrder"),
                        ContentSource = (string)elem.Element(NcxElements.Content)?.Attribute("src")
                    }).ToList().AsReadOnly()
                }
            };
            
            return ncx;
        }

        private static EpubNcxNavigationPoint ReadNavigationPoint(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (element.Name != NcxElements.NavPoint) throw new ArgumentException("The element is not <navPoint>", nameof(element));

            return new EpubNcxNavigationPoint
            {
                Id = (string)element.Attribute("id"),
                Class = (string)element.Attribute("class"),
                LabelText = element.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                ContentSrc = (string)element.Element(NcxElements.Content)?.Attribute("src"),
                PlayOrder = (int?)element.Attribute("playOrder"),
                NavigationPoints = element.Elements(NcxElements.NavPoint).Select(ReadNavigationPoint).ToList().AsReadOnly()
            };
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
                    case "playorder":
                        result.PlayOrder = int.Parse(attributeValue);
                        break;
                }
            }
            if (string.IsNullOrWhiteSpace(result.Id))
                throw new Exception("Incorrect EPUB navigation point: point ID is missing");
            var navigationPoints = new List<EpubNcxNavigationPoint>();
            foreach (XmlNode navigationPointChildNode in navigationPointNode.ChildNodes)
            {
                switch (navigationPointChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        result.LabelText = ReadNavigationLabel(navigationPointChildNode);
                        break;
                    case "content":
                        result.ContentSrc = ReadNavigationContent(navigationPointChildNode);
                        break;
                    case "navpoint":
                        navigationPoints.Add(ReadNavigationPoint(navigationPointChildNode));
                        break;
                }
            }
            result.NavigationPoints = navigationPoints.AsReadOnly();
            if (!result.LabelText.Any())
                throw new Exception($"EPUB parsing error: navigation point {result.Id} should contain at least one navigation label");
            if (result.ContentSrc == null)
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

        private static IReadOnlyCollection<EpubNcxPageTarget> ReadNavigationPageList(XmlNode navigationPageListNode)
        {
            return (from XmlNode pageTargetNode in navigationPageListNode.ChildNodes
                    where string.Compare(pageTargetNode.LocalName, "pageTarget", StringComparison.OrdinalIgnoreCase) == 0
                    select ReadNavigationPageTarget(pageTargetNode)).ToList().AsReadOnly();
        }

        private static EpubNcxPageTarget ReadNavigationPageTarget(XmlNode navigationPageTargetNode)
        {
            var result = new EpubNcxPageTarget();
            foreach (XmlAttribute navigationPageTargetNodeAttribute in navigationPageTargetNode.Attributes)
            {
                var attributeValue = navigationPageTargetNodeAttribute.Value;
                switch (navigationPageTargetNodeAttribute.Name.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "value":
                        result.Value = int.Parse(attributeValue);
                        break;
                    case "type":
                        EpubNcxPageTargetType type;
                        if (!Enum.TryParse(attributeValue, true, out type))
                            throw new Exception($"Incorrect EPUB navigation page target: {attributeValue} is incorrect value for page target type");
                        result.Type = type;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                }
            }
            if (result.Type == default(EpubNcxPageTargetType))
            {
                throw new Exception("Incorrect EPUB navigation page target: page target type is missing");
            }

            foreach (XmlNode navigationPageTargetChildNode in navigationPageTargetNode.ChildNodes)
            {
                switch (navigationPageTargetChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        result.Label = ReadNavigationLabel(navigationPageTargetChildNode);
                        break;
                    case "content":
                        result.ContentSource = ReadNavigationContent(navigationPageTargetChildNode);
                        break;
                }
            }
            if (!result.Label.Any())
                throw new Exception("Incorrect EPUB navigation page target: at least one navLabel element is required");
            return result;
        }

        private static EpubNcxNavigationList ReadNavigationList(XmlNode navigationListNode)
        {
            var result = new EpubNcxNavigationList();
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
            var navigationTargets = new List<EpubNcxNavigationTarget>();
            foreach (XmlNode navigationListChildNode in navigationListNode.ChildNodes)
            {
                switch (navigationListChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        result.Label = ReadNavigationLabel(navigationListChildNode);
                        break;
                    case "navTarget":
                        navigationTargets.Add(ReadNavigationTarget(navigationListChildNode));
                        break;
                }
            }
            result.NavigationTargets = navigationTargets.AsReadOnly();
            if (!result.Label.Any())
                throw new Exception("Incorrect EPUB navigation page target: at least one navLabel element is required");
            return result;
        }

        private static EpubNcxNavigationTarget ReadNavigationTarget(XmlNode navigationTargetNode)
        {
            var result = new EpubNcxNavigationTarget();
            foreach (XmlAttribute navigationPageTargetNodeAttribute in navigationTargetNode.Attributes)
            {
                var attributeValue = navigationPageTargetNodeAttribute.Value;
                switch (navigationPageTargetNodeAttribute.Name.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                    case "playOrder":
                        result.PlayOrder = int.Parse(attributeValue);
                        break;
                }
            }
            if (string.IsNullOrWhiteSpace(result.Id))
                throw new Exception("Incorrect EPUB navigation target: navigation target ID is missing");
            foreach (XmlNode navigationTargetChildNode in navigationTargetNode.ChildNodes)
            {
                switch (navigationTargetChildNode.LocalName.ToLowerInvariant())
                {
                    case "navlabel":
                        result.Label = ReadNavigationLabel(navigationTargetChildNode);
                        break;
                    case "content":
                        var content = ReadNavigationContent(navigationTargetChildNode);
                        result.ContentSource = content;
                        break;
                }
            }
            if (!result.Label.Any())
                throw new Exception("Incorrect EPUB navigation target: at least one navLabel element is required");
            return result;
        }
    }
}
