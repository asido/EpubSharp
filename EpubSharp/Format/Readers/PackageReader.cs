using System;
using System.Linq;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class PackageReader
    {
        private static readonly XNamespace PackageNamespace = "http://www.idpf.org/2007/opf";
        private static readonly XNamespace MetadataNamespace = "http://purl.org/dc/elements/1.1/";

        private static class PackageElements
        {
            public static readonly XName Metadata = PackageNamespace + "metadata";
            public static readonly XName Contributor = MetadataNamespace + "contributor";
            public static readonly XName Coverages = MetadataNamespace + "coverages";
            public static readonly XName Creator = MetadataNamespace + "creator";
            public static readonly XName Date = MetadataNamespace + "date";
            public static readonly XName Description = MetadataNamespace + "description";
            public static readonly XName Format = MetadataNamespace + "format";
            public static readonly XName Identifier = MetadataNamespace + "identifier";
            public static readonly XName Language = MetadataNamespace + "language";
            public static readonly XName Meta = PackageNamespace + "meta";
            public static readonly XName Publisher = MetadataNamespace + "publisher";
            public static readonly XName Relation = MetadataNamespace + "relation";
            public static readonly XName Rights = MetadataNamespace + "rights";
            public static readonly XName Source = MetadataNamespace + "source";
            public static readonly XName Subject = MetadataNamespace + "subject";
            public static readonly XName Title = MetadataNamespace + "title";
            public static readonly XName Type = MetadataNamespace + "type";

            public static readonly XName Guide = PackageNamespace + "guide";
            public static readonly XName Reference = PackageNamespace + "reference";

            public static readonly XName Manifest = PackageNamespace + "manifest";
            public static readonly XName Item = PackageNamespace + "item";

            public static readonly XName Spine = PackageNamespace + "spine";
            public static readonly XName ItemRef = PackageNamespace + "itemref";
        }

        public static PackageDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            Func<XElement, PackageMetadataCreator> readCreator = elem => new PackageMetadataCreator
            {
                Role = (string) elem.Attribute(PackageNamespace + "role"),
                FileAs = (string) elem.Attribute(PackageNamespace + "file-as"),
                AlternateScript = (string) elem.Attribute(PackageNamespace + "alternate-script"),
                Text = elem.Value
            };

            var epubVersion = GetAndValidateVersion((string) xml.Root.Attribute("version"));
            var metadata = xml.Root.Element(PackageElements.Metadata);
            var guide = xml.Root.Element(PackageElements.Guide);
            var spine = xml.Root.Element(PackageElements.Spine);

            var package = new PackageDocument
            {
                EpubVersion = epubVersion,
                Metadata = new PackageMetadata
                {
                    Creators = metadata?.Elements(PackageElements.Creator).AsObjectList(readCreator),
                    Contributors = metadata?.Elements(PackageElements.Contributor).AsObjectList(readCreator),
                    Coverages = metadata?.Elements(PackageElements.Coverages).AsStringList(),
                    Dates = metadata?.Elements(PackageElements.Date).AsObjectList(elem => new PackageMetadataDate
                    {
                        Text = elem.Value,
                        Event = (string)elem.Attribute(PackageNamespace + "event")
                    }),
                    Descriptions = metadata?.Elements(PackageElements.Description).AsStringList(),
                    Formats = metadata?.Elements(PackageElements.Format).AsStringList(),
                    Identifiers = metadata?.Elements(PackageElements.Identifier).AsObjectList(elem => new PackageMetadataIdentifier
                    {
                        Id = (string) elem.Attribute("id"),
                        Scheme = (string) elem.Attribute(PackageNamespace + "scheme"),
                        Text = elem.Value
                    }),
                    Languages = metadata?.Elements(PackageElements.Language).AsStringList(),
                    Metas = metadata?.Elements(PackageElements.Meta).AsObjectList(elem => new PackageMetadataMeta
                    {
                        Id = (string) elem.Attribute("id"),
                        Name = (string) elem.Attribute("name"),
                        Refines = (string) elem.Attribute("refines"),
                        Scheme = (string) elem.Attribute("scheme"),
                        Property = (string) elem.Attribute("property"),
                        Text = epubVersion == EpubVersion.Epub2 ? (string) elem.Attribute("content") : elem.Value
                    }),
                    Publishers = metadata?.Elements(PackageElements.Publisher).AsStringList(),
                    Relations = metadata?.Elements(PackageElements.Relation).AsStringList(),
                    Rights = metadata?.Elements(PackageElements.Rights).AsStringList(),
                    Sources = metadata?.Elements(PackageElements.Source).AsStringList(),
                    Subjects = metadata?.Elements(PackageElements.Subject).AsStringList(),
                    Titles = metadata?.Elements(PackageElements.Title).AsStringList(),
                    Types = metadata?.Elements(PackageElements.Type).AsStringList()
                },
                Guide = guide == null ? null : new PackageGuide
                {
                    References = guide.Elements(PackageElements.Reference)?.AsObjectList(elem => new PackageGuideReference
                    {
                        Title = (string) elem.Attribute("title"),
                        Type = (string) elem.Attribute("type"),
                        Href = (string) elem.Attribute("href")
                    })
                },
                Manifest = new PackageManifest
                {
                    Items = xml.Root.Element(PackageElements.Manifest)?.Elements(PackageElements.Item).AsObjectList(elem => new PackageManifestItem
                    {
                        Fallback = (string) elem.Attribute("fallback"),
                        FallbackStyle = (string) elem.Attribute("fallback-style"),
                        Href = (string) elem.Attribute("href"),
                        Id = (string) elem.Attribute("id"),
                        MediaType = (string) elem.Attribute("media-type"),
                        Properties = ((string) elem.Attribute("properties"))?.Split(' ') ?? new string[0],
                        RequiredModules = (string) elem.Attribute("required-modules"),
                        RequiredNamespace = (string) elem.Attribute("required-namespace")
                    })
                },
                Spine = new PackageSpine
                {
                    ItemRefs = spine?.Elements(PackageElements.ItemRef).AsObjectList(elem => new PackageSpineItemRef
                    {
                        IdRef = (string) elem.Attribute("idref"),
                        Linear = (string) elem.Attribute("linear") != "no",
                        Id = (string) elem.Attribute("id"),
                        Properties = ((string) elem.Attribute("properties"))?.Split(' ')
                    }),
                    Toc = spine?.Attribute("toc")?.Value
                }
            };

            package.NavPath = FindNavPath(package);
            package.NcxPath = FindNcxPath(package);
            package.CoverPath = FindCoverPath(package);

            return package;
        }

        private static EpubVersion GetAndValidateVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentNullException(nameof(version));

            if (version == "2.0")
            {
                return EpubVersion.Epub2;
            }
            if (version == "3.0" || version == "3.0.1" || version == "3.1")
            {
                return EpubVersion.Epub3;
            }

            throw new Exception($"Unsupported EPUB version: {version}.");
        }

        private static string FindCoverPath(PackageDocument package)
        {
            string coverId = null;

            var coverMetaItem = package.Metadata.Metas
                .FirstOrDefault(metaItem => string.Compare(metaItem.Name, "cover", StringComparison.OrdinalIgnoreCase) == 0);
            if (coverMetaItem != null)
            {
                coverId = coverMetaItem.Text;
            }
            else
            {
                var item = package.Manifest.Items.FirstOrDefault(e => e.Properties.Contains("cover-image"));
                if (item != null)
                {
                    coverId = item.Href;
                }
            }

            if (coverId == null)
            {
                return null;
            }

            var coverItem = package.Manifest.Items.FirstOrDefault(item => item.Id == coverId);
            return coverItem?.Href;
        }

        private static string FindNcxPath(PackageDocument package)
        {
            var ncxItem = package.Manifest.Items.FirstOrDefault(e => e.MediaType == "application/x-dtbncx+xml");
            if (ncxItem != null)
            {
                package.NcxPath = ncxItem.Href;
            }
            else
            {
                // If we can't find the toc by media-type then try to look for id of the item in the spine attributes as
                // according to http://www.idpf.org/epub/20/spec/OPF_2.0.1_draft.htm#Section2.4.1.2,
                // "The item that describes the NCX must be referenced by the spine toc attribute."

                if (!string.IsNullOrWhiteSpace(package.Spine.Toc))
                {
                    var tocItem = package.Manifest.Items.FirstOrDefault(e => e.Id == package.Spine.Toc);
                    if (tocItem != null)
                    {
                        return tocItem.Href;
                    }
                }
            }

            return null;
        }

        private static string FindNavPath(PackageDocument package)
        {
            var navItem = package.Manifest.Items.FirstOrDefault(e => e.Properties.Contains("nav"));
            return navItem?.Href;
        }
    }
}
