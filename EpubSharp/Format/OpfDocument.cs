using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EpubSharp.Format
{
    internal static class OpfElements
    {
        public static readonly XName Package = Constants.OpfNamespace + "package";

        public static readonly XName Metadata = Constants.OpfNamespace + "metadata";
        public static readonly XName Contributor = Constants.OpfMetadataNamespace + "contributor";
        public static readonly XName Coverages = Constants.OpfMetadataNamespace + "coverages";
        public static readonly XName Creator = Constants.OpfMetadataNamespace + "creator";
        public static readonly XName Date = Constants.OpfMetadataNamespace + "date";
        public static readonly XName Description = Constants.OpfMetadataNamespace + "description";
        public static readonly XName Format = Constants.OpfMetadataNamespace + "format";
        public static readonly XName Identifier = Constants.OpfMetadataNamespace + "identifier";
        public static readonly XName Language = Constants.OpfMetadataNamespace + "language";
        public static readonly XName Meta = Constants.OpfNamespace + "meta";
        public static readonly XName Publisher = Constants.OpfMetadataNamespace + "publisher";
        public static readonly XName Relation = Constants.OpfMetadataNamespace + "relation";
        public static readonly XName Rights = Constants.OpfMetadataNamespace + "rights";
        public static readonly XName Source = Constants.OpfMetadataNamespace + "source";
        public static readonly XName Subject = Constants.OpfMetadataNamespace + "subject";
        public static readonly XName Title = Constants.OpfMetadataNamespace + "title";
        public static readonly XName Type = Constants.OpfMetadataNamespace + "type";

        public static readonly XName Guide = Constants.OpfNamespace + "guide";
        public static readonly XName Reference = Constants.OpfNamespace + "reference";

        public static readonly XName Manifest = Constants.OpfNamespace + "manifest";
        public static readonly XName Item = Constants.OpfNamespace + "item";

        public static readonly XName Spine = Constants.OpfNamespace + "spine";
        public static readonly XName ItemRef = Constants.OpfNamespace + "itemref";
    }

    public enum EpubVersion
    {
        Epub2 = 2,
        Epub3
    }
    
    public class OpfDocument
    {
        internal static class Attributes
        {
            public static readonly XName Version = "version";
        }

        public EpubVersion EpubVersion { get; internal set; } = new EpubVersion();
        public OpfMetadata Metadata { get; internal set; } = new OpfMetadata();
        public OpfManifest Manifest { get; internal set; } = new OpfManifest();
        public OpfSpine Spine { get; internal set; } = new OpfSpine();
        public OpfGuide Guide { get; internal set; } = new OpfGuide();

        public string FindCoverPath()
        {
            string coverId = null;

            var coverMetaItem = Metadata.Metas
                .FirstOrDefault(metaItem => string.Compare(metaItem.Name, "cover", StringComparison.OrdinalIgnoreCase) == 0);
            if (coverMetaItem != null)
            {
                coverId = coverMetaItem.Text;
            }
            else
            {
                var item = Manifest.Items.FirstOrDefault(e => e.Properties.Contains("cover-image"));
                if (item != null)
                {
                    coverId = item.Href;
                }
            }

            if (coverId == null)
            {
                return null;
            }

            var coverItem = Manifest.Items.FirstOrDefault(item => item.Id == coverId);
            return coverItem?.Href;
        }

        public string FindNcxPath()
        {
            string path = null;

            var ncxItem = Manifest.Items.FirstOrDefault(e => e.MediaType == "application/x-dtbncx+xml");
            if (ncxItem != null)
            {
                path = ncxItem.Href;
            }
            else
            {
                // If we can't find the toc by media-type then try to look for id of the item in the spine attributes as
                // according to http://www.idpf.org/epub/20/spec/OPF_2.0.1_draft.htm#Section2.4.1.2,
                // "The item that describes the NCX must be referenced by the spine toc attribute."

                if (!string.IsNullOrWhiteSpace(Spine.Toc))
                {
                    var tocItem = Manifest.Items.FirstOrDefault(e => e.Id == Spine.Toc);
                    if (tocItem != null)
                    {
                        path = tocItem.Href;
                    }
                }
            }

            return path;
        }

        public string FindNavPath()
        {
            var navItem = Manifest.Items.FirstOrDefault(e => e.Properties.Contains("nav"));
            return navItem?.Href;
        }
    }

    public class OpfMetadata
    {
        public ICollection<string> Titles { get; internal set; } = new List<string>();
        public ICollection<string> Subjects { get; internal set; } = new List<string>();
        public ICollection<string> Descriptions { get; internal set; } = new List<string>();
        public ICollection<string> Publishers { get; internal set; } = new List<string>();
        public ICollection<OpfMetadataCreator> Creators { get; internal set; } = new List<OpfMetadataCreator>();
        public ICollection<OpfMetadataCreator> Contributors { get; internal set; } = new List<OpfMetadataCreator>();
        public ICollection<OpfMetadataDate> Dates { get; internal set; } = new List<OpfMetadataDate>();
        public ICollection<string> Types { get; internal set; } = new List<string>();
        public ICollection<string> Formats { get; internal set; } = new List<string>();
        public ICollection<OpfMetadataIdentifier> Identifiers { get; internal set; } = new List<OpfMetadataIdentifier>();
        public ICollection<string> Sources { get; internal set; } = new List<string>();
        public ICollection<string> Languages { get; internal set; } = new List<string>();
        public ICollection<string> Relations { get; internal set; } = new List<string>();
        public ICollection<string> Coverages { get; internal set; } = new List<string>();
        public ICollection<string> Rights { get; internal set; } = new List<string>();
        public ICollection<OpfMetadataMeta> Metas { get; internal set; } = new List<OpfMetadataMeta>();
    }

    public class OpfMetadataDate
    {
        internal static class Attributes
        {
            public static readonly XName Event = Constants.OpfNamespace + "event";
        }

        public string Text { get; internal set; }

        /// <summary>
        /// i.e. "modification"
        /// </summary>
        public string Event { get; internal set; }
    }

    public class OpfMetadataCreator
    {
        internal static class Attributes
        {
            public static readonly XName Role = Constants.OpfNamespace + "role";
            public static readonly XName FileAs = Constants.OpfNamespace + "file-as";
            public static readonly XName AlternateScript = Constants.OpfNamespace + "alternate-script";
        }

        public string Text { get; internal set; }
        public string Role { get; internal set; }
        public string FileAs { get; internal set; }
        public string AlternateScript { get; internal set; }
    }

    public class OpfMetadataIdentifier
    {
        internal static class Attributes
        {
            public static readonly XName Id = "id";
            public static readonly XName Scheme = "scheme";
        }

        public string Id { get; internal set; }
        public string Scheme { get; internal set; }
        public string Text { get; internal set; }
    }

    public class OpfMetadataMeta
    {
        internal static class Attributes
        {
            public static readonly XName Id = "id";
            public static readonly XName Name = "name";
            public static readonly XName Refines = "refines";
            public static readonly XName Scheme = "scheme";
            public static readonly XName Property = "property";
            public static readonly XName Content = "content";
        }

        public string Name { get; internal set; }
        public string Id { get; internal set; }
        public string Refines { get; internal set; }
        public string Property { get; internal set; }
        public string Scheme { get; internal set; }
        public string Text { get; internal set; }
    }

    public class OpfManifest
    {
        public ICollection<OpfManifestItem> Items { get; internal set; } = new List<OpfManifestItem>();
    }

    public class OpfManifestItem
    {
        internal static class Attributes
        {
            public static readonly XName Fallback = "fallback";
            public static readonly XName FallbackStyle = "fallback-style";
            public static readonly XName Href = "href";
            public static readonly XName Id = "id";
            public static readonly XName MediaType = "media-type";
            public static readonly XName Properties = "properties";
            public static readonly XName RequiredModules = "required-modules";
            public static readonly XName RequiredNamespace = "required-namespace";
        }

        public string Id { get; internal set; }
        public string Href { get; internal set; }
        public ICollection<string> Properties { get; internal set; } = new List<string>();
        public string MediaType { get; internal set; }
        public string RequiredNamespace { get; internal set; }
        public string RequiredModules { get; internal set; }
        public string Fallback { get; internal set; }
        public string FallbackStyle { get; internal set; }

        public override string ToString()
        {
            return $"Id: {Id}, Href = {Href}, MediaType = {MediaType}";
        }
    }

    public class OpfSpine
    {
        internal static class Attributes
        {
            public static readonly XName Toc = "toc";
        }

        public string Toc { get; internal set; }
        public ICollection<OpfSpineItemRef> ItemRefs { get; internal set; } = new List<OpfSpineItemRef>();
    }

    public class OpfSpineItemRef
    {
        internal static class Attributes
        {
            public static readonly XName IdRef = "idref";
            public static readonly XName Linear = "linear";
            public static readonly XName Id = "id";
            public static readonly XName Properties = "properties";
        }

        public string IdRef { get; internal set; }
        public bool Linear { get; internal set; }
        public string Id { get; internal set; }
        public ICollection<string> Properties { get; internal set; } = new List<string>();

        public override string ToString()
        {
            return "IdRef: " + IdRef;
        }
    }

    public class OpfGuide
    {
        public ICollection<OpfGuideReference> References { get; internal set; } = new List<OpfGuideReference>();
    }

    public class OpfGuideReference
    {
        internal static class Attributes
        {
            public static readonly XName Title = "title";
            public static readonly XName Type = "type";
            public static readonly XName Href = "href";
        }

        public string Type { get; internal set; }
        public string Title { get; internal set; }
        public string Href { get; internal set; }

        public override string ToString()
        {
            return $"Type: {Type}, Href: {Href}";
        }
    }
}
