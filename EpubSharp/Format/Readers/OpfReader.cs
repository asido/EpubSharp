using System;
using System.Linq;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class OpfReader
    {
        private static class OpfElements
        {
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

        public static OpfDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            Func<XElement, OpfMetadataCreator> readCreator = elem => new OpfMetadataCreator
            {
                Role = (string) elem.Attribute(Constants.OpfNamespace + "role"),
                FileAs = (string) elem.Attribute(Constants.OpfNamespace + "file-as"),
                AlternateScript = (string) elem.Attribute(Constants.OpfNamespace + "alternate-script"),
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
                        Event = (string)elem.Attribute(Constants.OpfNamespace + "event")
                    }),
                    Descriptions = metadata?.Elements(OpfElements.Description).AsStringList(),
                    Formats = metadata?.Elements(OpfElements.Format).AsStringList(),
                    Identifiers = metadata?.Elements(OpfElements.Identifier).AsObjectList(elem => new OpfMetadataIdentifier
                    {
                        Id = (string) elem.Attribute("id"),
                        Scheme = (string) elem.Attribute(Constants.OpfNamespace + "scheme"),
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
    }
}
