using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class NavReader
    {
        public static NavDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            var ns = xml.Root.Name.Namespace;

            var head = xml.Root?.Element(ns + NavElements.Head);
            var body = xml.Root?.Element(ns + NavElements.Body);

            var nav = new NavDocument
            {
                Head = new NavHead
                {
                    Dom = head,
                    Title = head?.Element(ns + NavElements.Title)?.Value,
                    Links = head?.Elements(ns + NavElements.Link).AsObjectList(elem => new NavHeadLink
                    {
                        Class = elem.Attribute(NavHeadLink.Attributes.Class)?.Value,
                        Href = elem.Attribute(NavHeadLink.Attributes.Href)?.Value,
                        Rel = elem.Attribute(NavHeadLink.Attributes.Rel)?.Value,
                        Title = elem.Attribute(NavHeadLink.Attributes.Title)?.Value,
                        Type = elem.Attribute(NavHeadLink.Attributes.Type)?.Value,
                        Media = elem.Attribute(NavHeadLink.Attributes.Media)?.Value
                    }) ?? new List<NavHeadLink>(),
                    Metas = head?.Elements(ns + NavElements.Meta).AsObjectList(elem => new NavMeta
                    {
                        Name = elem.Attribute(NavMeta.Attributes.Name)?.Value,
                        Content = elem.Attribute(NavMeta.Attributes.Content)?.Value,
                        Charset = elem.Attribute(NavMeta.Attributes.Charset)?.Value
                    }) ?? new List<NavMeta>()
                },
                Body = new NavBody
                {
                    Dom = body,
                    Navs = body?.Descendants(ns + NavElements.Nav).AsObjectList(elem => new NavNav
                    {
                        Dom = elem,
                        Id = elem.Attribute(NavNav.Attributes.Id)?.Value,
                        Class = elem.Attribute(NavNav.Attributes.Class)?.Value,
                        Hidden = elem.Attribute(NavNav.Attributes.Hidden)?.Value,
                        Type = elem.Attribute(NavNav.Attributes.Type)?.Value
                    })
                }
            };

            return nav;
        }
    }
}
