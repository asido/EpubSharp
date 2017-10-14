using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class OpfReader
    {
        public static OpfDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            Func<XElement, OpfMetadataCreator> readCreator = elem => new OpfMetadataCreator
            {
                Role = (string) elem.Attribute(OpfMetadataCreator.Attributes.Role),
                FileAs = (string) elem.Attribute(OpfMetadataCreator.Attributes.FileAs),
                AlternateScript = (string) elem.Attribute(OpfMetadataCreator.Attributes.AlternateScript),
                Text = elem.Value
            };

            var epubVersion = GetAndValidateVersion((string) xml.Root.Attribute(OpfDocument.Attributes.Version));
            var metadata = xml.Root.Element(OpfElements.Metadata);
            var guide = xml.Root.Element(OpfElements.Guide);
            var spine = xml.Root.Element(OpfElements.Spine);

            var package = new OpfDocument
            {
                UniqueIdentifier = (string) xml.Root.Attribute(OpfDocument.Attributes.UniqueIdentifier),
                EpubVersion = epubVersion,
                Metadata = new OpfMetadata
                {
                    Creators = metadata?.Elements(OpfElements.Creator).AsObjectList(readCreator),
                    Contributors = metadata?.Elements(OpfElements.Contributor).AsObjectList(readCreator),
                    Coverages = metadata?.Elements(OpfElements.Coverages).AsStringList(),
                    Dates = metadata?.Elements(OpfElements.Date).AsObjectList(elem => new OpfMetadataDate
                    {
                        Text = elem.Value,
                        Event = (string)elem.Attribute(OpfMetadataDate.Attributes.Event)
                    }),
                    Descriptions = metadata?.Elements(OpfElements.Description).AsStringList(),
                    Formats = metadata?.Elements(OpfElements.Format).AsStringList(),
                    Identifiers = metadata?.Elements(OpfElements.Identifier).AsObjectList(elem => new OpfMetadataIdentifier
                    {
                        Id = (string) elem.Attribute(OpfMetadataIdentifier.Attributes.Id),
                        Scheme = (string) elem.Attribute(OpfMetadataIdentifier.Attributes.Scheme),
                        Text = elem.Value
                    }),
                    Languages = metadata?.Elements(OpfElements.Language).AsStringList(),
                    Metas = metadata?.Elements(OpfElements.Meta).AsObjectList(elem => new OpfMetadataMeta
                    {
                        Id = (string) elem.Attribute(OpfMetadataMeta.Attributes.Id),
                        Name = (string) elem.Attribute(OpfMetadataMeta.Attributes.Name),
                        Refines = (string) elem.Attribute(OpfMetadataMeta.Attributes.Refines),
                        Scheme = (string) elem.Attribute(OpfMetadataMeta.Attributes.Scheme),
                        Property = (string) elem.Attribute(OpfMetadataMeta.Attributes.Property),
                        Text = epubVersion == EpubVersion.Epub2 ? (string) elem.Attribute(OpfMetadataMeta.Attributes.Content) : elem.Value
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
                    References = guide.Elements(OpfElements.Reference).AsObjectList(elem => new OpfGuideReference
                    {
                        Title = (string) elem.Attribute(OpfGuideReference.Attributes.Title),
                        Type = (string) elem.Attribute(OpfGuideReference.Attributes.Type),
                        Href = (string) elem.Attribute(OpfGuideReference.Attributes.Href)
                    })
                },
                Manifest = new OpfManifest
                {
                    Items = xml.Root.Element(OpfElements.Manifest)?.Elements(OpfElements.Item).AsObjectList(elem => new OpfManifestItem
                    {
                        Fallback = (string) elem.Attribute(OpfManifestItem.Attributes.Fallback),
                        FallbackStyle = (string) elem.Attribute(OpfManifestItem.Attributes.FallbackStyle),
                        Href = (string) elem.Attribute(OpfManifestItem.Attributes.Href),
                        Id = (string) elem.Attribute(OpfManifestItem.Attributes.Id),
                        MediaType = (string) elem.Attribute(OpfManifestItem.Attributes.MediaType),
                        Properties = ((string) elem.Attribute(OpfManifestItem.Attributes.Properties))?.Split(' ') ?? new string[0],
                        RequiredModules = (string) elem.Attribute(OpfManifestItem.Attributes.RequiredModules),
                        RequiredNamespace = (string) elem.Attribute(OpfManifestItem.Attributes.RequiredNamespace)
                    })
                },
                Spine = new OpfSpine
                {
                    ItemRefs = spine?.Elements(OpfElements.ItemRef).AsObjectList(elem => new OpfSpineItemRef
                    {
                        IdRef = (string) elem.Attribute(OpfSpineItemRef.Attributes.IdRef),
                        Linear = (string) elem.Attribute(OpfSpineItemRef.Attributes.Linear) != "no",
                        Id = (string) elem.Attribute(OpfSpineItemRef.Attributes.Id),
                        Properties = ((string) elem.Attribute(OpfSpineItemRef.Attributes.Properties))?.Split(' ').ToList() ?? new List<string>()
                    }),
                    Toc = spine?.Attribute(OpfSpine.Attributes.Toc)?.Value
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
