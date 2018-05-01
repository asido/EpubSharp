using System.Collections.Generic;
using System.Xml.Linq;

namespace EpubSharp.Format
{
    internal static class NavElements
    {
        public static readonly string Html = "html";

        public static readonly string Head = "head";
        public static readonly string Title = "title";
        public static readonly string Link = "link";
        public static readonly string Meta = "meta";

        public static readonly string Body = "body";
        public static readonly string Nav = "nav";
        public static readonly string Ol = "ol";
        public static readonly string Li = "li";
        public static readonly string A = "a";
    }

    public class NavDocument
    {
        public NavHead Head { get; internal set; } = new NavHead();
        public NavBody Body { get; internal set; } = new NavBody();
    }

    public class NavHead
    {
        /// <summary>
        /// Instantiated only when the EPUB was read.
        /// </summary>
        internal XElement Dom { get; set; }

        public string Title { get; internal set; }
        public IList<NavHeadLink> Links { get; internal set; } = new List<NavHeadLink>();
        public IList<NavMeta> Metas { get; internal set; } = new List<NavMeta>();
    }

    public class NavHeadLink
    {
        internal static class Attributes
        {
            public static readonly XName Href = "href";
            public static readonly XName Rel = "rel";
            public static readonly XName Type = "type";
            public static readonly XName Class = "class";
            public static readonly XName Title = "title";
            public static readonly XName Media = "media";
        }

        public string Href { get; internal set; }
        public string Rel { get; internal set; }
        public string Type { get; internal set; }
        public string Class { get; internal set; }
        public string Title { get; internal set; }
        public string Media { get; internal set; }
    }

    public class NavMeta
    {
        internal static class Attributes
        {
            public static readonly XName Name = "name";
            public static readonly XName Content = "content";
            public static readonly XName Charset = "charset";
        }

        public string Name { get; internal set; }
        public string Content { get; internal set; }
        public string Charset { get; internal set; }
    }

    public class NavBody
    {
        /// <summary>
        /// Instantiated only when the EPUB was read.
        /// </summary>
        internal XElement Dom { get; set; }

        public IList<NavNav> Navs { get; internal set; } = new List<NavNav>();
    }

    public class NavNav
    {
        internal static class Attributes
        {
            public static readonly XName Id = "id";
            public static readonly XName Class = "class";
            public static readonly XName Type = Constants.OpsNamespace + "type";
            public static readonly XName Hidden = Constants.OpsNamespace + "hidden";

            internal static class TypeValues
            {
                public const string Toc = "toc";
                public const string Landmarks = "landmarks";
                public const string PageList = "page-list";
            }
        }

        /// <summary>
        /// Instantiated only when the EPUB was read.
        /// </summary>
        internal XElement Dom { get; set; }

        public string Type { get; internal set; }
        public string Id { get; internal set; }
        public string Class { get; internal set; }
        public string Hidden { get; internal set; }
    }
}
