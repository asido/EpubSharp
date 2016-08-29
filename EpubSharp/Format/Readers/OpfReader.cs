using System;
using System.Linq;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class OpfReader
    {
        private static readonly XNamespace OpfNamespace = "http://www.idpf.org/2007/opf";
        private static readonly XNamespace MetadataNamespace = "http://purl.org/dc/elements/1.1/";

        private static class OpfElements
        {
            public static readonly XName Metadata = OpfNamespace + "metadata";
            public static readonly XName Contributor = MetadataNamespace + "contributor";
            public static readonly XName Coverages = MetadataNamespace + "coverages";
            public static readonly XName Creator = MetadataNamespace + "creator";
            public static readonly XName Date = MetadataNamespace + "date";
            public static readonly XName Description = MetadataNamespace + "description";
            public static readonly XName Format = MetadataNamespace + "format";
            public static readonly XName Identifier = MetadataNamespace + "identifier";
            public static readonly XName Language = MetadataNamespace + "language";
            public static readonly XName Meta = OpfNamespace + "meta";
            public static readonly XName Publisher = MetadataNamespace + "publisher";
            public static readonly XName Relation = MetadataNamespace + "relation";
            public static readonly XName Rights = MetadataNamespace + "rights";
            public static readonly XName Source = MetadataNamespace + "source";
            public static readonly XName Subject = MetadataNamespace + "subject";
            public static readonly XName Title = MetadataNamespace + "title";
            public static readonly XName Type = MetadataNamespace + "type";

            public static readonly XName Guide = OpfNamespace + "guide";
            public static readonly XName Reference = OpfNamespace + "reference";

            public static readonly XName Manifest = OpfNamespace + "manifest";
            public static readonly XName Item = OpfNamespace + "item";

            public static readonly XName Spine = OpfNamespace + "spine";
            public static readonly XName ItemRef = OpfNamespace + "itemref";
        }

        public static OpfDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            Func<XElement, OpfMetadataCreator> readCreator = elem => new OpfMetadataCreator
            {
                Role = (string) elem.Attribute(OpfNamespace + "role"),
                FileAs = (string) elem.Attribute(OpfNamespace + "file-as"),
                AlternateScript = (string) elem.Attribute(OpfNamespace + "alternate-script"),
                Text = elem.Value
            };

            var epubVersion = GetAndValidateVersion((string) xml.Root.Attribute("version"));
            var metadata = xml.Root.Element(OpfElements.Metadata);
            var guide = xml.Root.Element(OpfElements.Guide);
            var spine = xml.Root.Element(OpfElements.Spine);

            var package = new OpfDocument
            {
                EpubVersion = epubVersion,
                Metadata = new OpfMetadata
                {
                    Creators = metadata?.Elements(OpfElements.Creator).AsObjectList(readCreator),
                    Contributors = metadata?.Elements(OpfElements.Contributor).AsObjectList(readCreator),
                    Coverages = metadata?.Elements(OpfElements.Coverages).AsStringList(),
                    Dates = metadata?.Elements(OpfElements.Date).AsObjectList(elem => new OpfMetadataDate
                    {
                        Text = elem.Value,
                        Event = (string)elem.Attribute(OpfNamespace + "event")
                    }),
                    Descriptions = metadata?.Elements(OpfElements.Description).AsStringList(),
                    Formats = metadata?.Elements(OpfElements.Format).AsStringList(),
                    Identifiers = metadata?.Elements(OpfElements.Identifier).AsObjectList(elem => new OpfMetadataIdentifier
                    {
                        Id = (string) elem.Attribute("id"),
                        Scheme = (string) elem.Attribute(OpfNamespace + "scheme"),
                        Text = elem.Value
                    }),
                    Languages = metadata?.Elements(OpfElements.Language).AsStringList(),
                    Metas = metadata?.Elements(OpfElements.Meta).AsObjectList(elem => new OpfMetadataMeta
                    {
                        Id = (string) elem.Attribute("id"),
                        Name = (string) elem.Attribute("name"),
                        Refines = (string) elem.Attribute("refines"),
                        Scheme = (string) elem.Attribute("scheme"),
                        Property = (string) elem.Attribute("property"),
                        Text = epubVersion == EpubVersion.Epub2 ? (string) elem.Attribute("content") : elem.Value
                    }),
                    Publishers = metadata?.Elements(OpfElements.Publisher).AsStringList(),
                    Relations = metadata?.Elements(OpfElements.Relation).AsStringList(),
                    Rights = metadata?.Elements(OpfElements.Rights).AsStringList(),
                    Sources = metadata?.Elements(OpfElements.Source).AsStringList(),
                    Subjects = metadata?.Elements(OpfElements.Subject).AsStringList(),
                    Titles = metadata?.Elements(OpfElements.Title).AsStringList(),
                    Types = metadata?.Elements(OpfElements.Type).AsStringList()
                },
                Guide = guide == null ? null : new OpfGuide
                {
                    References = guide.Elements(OpfElements.Reference)?.AsObjectList(elem => new OpfGuideReference
                    {
                        Title = (string) elem.Attribute("title"),
                        Type = (string) elem.Attribute("type"),
                        Href = (string) elem.Attribute("href")
                    })
                },
                Manifest = new OpfManifest
                {
                    Items = xml.Root.Element(OpfElements.Manifest)?.Elements(OpfElements.Item).AsObjectList(elem => new OpfManifestItem
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
                Spine = new OpfSpine
                {
                    ItemRefs = spine?.Elements(OpfElements.ItemRef).AsObjectList(elem => new OpfSpineItemRef
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

        private static string FindCoverPath(OpfDocument opf)
        {
            string coverId = null;

            var coverMetaItem = opf.Metadata.Metas
                .FirstOrDefault(metaItem => string.Compare(metaItem.Name, "cover", StringComparison.OrdinalIgnoreCase) == 0);
            if (coverMetaItem != null)
            {
                coverId = coverMetaItem.Text;
            }
            else
            {
                var item = opf.Manifest.Items.FirstOrDefault(e => e.Properties.Contains("cover-image"));
                if (item != null)
                {
                    coverId = item.Href;
                }
            }

            if (coverId == null)
            {
                return null;
            }

            var coverItem = opf.Manifest.Items.FirstOrDefault(item => item.Id == coverId);
            return coverItem?.Href;
        }

        private static string FindNcxPath(OpfDocument opf)
        {
            var ncxItem = opf.Manifest.Items.FirstOrDefault(e => e.MediaType == "application/x-dtbncx+xml");
            if (ncxItem != null)
            {
                opf.NcxPath = ncxItem.Href;
            }
            else
            {
                // If we can't find the toc by media-type then try to look for id of the item in the spine attributes as
                // according to http://www.idpf.org/epub/20/spec/OPF_2.0.1_draft.htm#Section2.4.1.2,
                // "The item that describes the NCX must be referenced by the spine toc attribute."

                if (!string.IsNullOrWhiteSpace(opf.Spine.Toc))
                {
                    var tocItem = opf.Manifest.Items.FirstOrDefault(e => e.Id == opf.Spine.Toc);
                    if (tocItem != null)
                    {
                        return tocItem.Href;
                    }
                }
            }

            return null;
        }

        private static string FindNavPath(OpfDocument opf)
        {
            var navItem = opf.Manifest.Items.FirstOrDefault(e => e.Properties.Contains("nav"));
            return navItem?.Href;
        }
    }
}
