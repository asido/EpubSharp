using System;
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

        public static NcxDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            var navList = xml.Root.Element(NcxElements.NavList);
            var ncx = new NcxDocument
            {
                Metadata = xml.Root.Element(NcxElements.Head)?.Elements(NcxElements.Meta).AsObjectList(elem => new NcxMetadata
                {
                    Name = (string)elem.Attribute("name"),
                    Content = (string)elem.Attribute("content"),
                    Scheme = (string)elem.Attribute("scheme")
                }),
                DocTitle = xml.Root.Element(NcxElements.DocTitle)?.Element(NcxElements.Text)?.Value,
                DocAuthor = xml.Root.Element(NcxElements.DocAuthor)?.Element(NcxElements.Text)?.Value,
                NavigationMap = xml.Root.Element(NcxElements.NavMap)?.Elements(NcxElements.NavPoint).AsObjectList(ReadNavigationPoint),
                PageList = xml.Root.Element(NcxElements.PageList)?.Elements(NcxElements.PageTarget).AsObjectList(elem => new NcxPageTarget
                {
                    Id = (string)elem.Attribute("id"),
                    Class = (string)elem.Attribute("class"),
                    Value = (int?)elem.Attribute("value"),
                    Type = (NcxPageTargetType?)(elem.Attribute("type") == null ? null : Enum.Parse(typeof(NcxPageTargetType), (string)elem.Attribute("type"), true)),
                    Label = elem.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                    ContentSource = (string)elem.Element(NcxElements.Content)?.Attribute("src")
                }),
                NavigationList = navList == null ? null : new NcxNavigationList
                {
                    Id = (string)navList.Attribute("id"),
                    Class = (string)navList.Attribute("class"),
                    Label = navList.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                    NavigationTargets = navList.Elements(NcxElements.NavTarget).AsObjectList(elem => new NcxNavigationTarget
                    {
                        Id = (string)elem.Attribute("id"),
                        Class = (string)elem.Attribute("class"),
                        Label = navList.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                        PlayOrder = (int?)elem.Attribute("playOrder"),
                        ContentSource = (string)elem.Element(NcxElements.Content)?.Attribute("src")
                    })
                }
            };
            
            return ncx;
        }

        private static NcxNavigationPoint ReadNavigationPoint(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (element.Name != NcxElements.NavPoint) throw new ArgumentException("The element is not <navPoint>", nameof(element));

            return new NcxNavigationPoint
            {
                Id = (string)element.Attribute("id"),
                Class = (string)element.Attribute("class"),
                LabelText = element.Element(NcxElements.NavLabel)?.Element(NcxElements.Text)?.Value,
                ContentSrc = (string)element.Element(NcxElements.Content)?.Attribute("src"),
                PlayOrder = (int?)element.Attribute("playOrder"),
                NavigationPoints = element.Elements(NcxElements.NavPoint).AsObjectList(ReadNavigationPoint)
            };
        }
    }
}
