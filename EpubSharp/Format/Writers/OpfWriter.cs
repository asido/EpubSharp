using System;
using System.Linq;
using System.Xml.Linq;

namespace EpubSharp.Format.Writers
{
    internal class OpfWriter
    {
        public static string Format(OpfDocument opf)
        {
            var root = new XElement(OpfElements.Package);
            root.Add(new XAttribute("xmlns", Constants.OpfNamespace));
            root.Add(new XAttribute(XNamespace.Xmlns + "dc", Constants.OpfMetadataNamespace));

            // This attribute is required, but some books don't have it. So we leave it as it is.
            if (!string.IsNullOrWhiteSpace(opf.UniqueIdentifier))
            {
                root.Add(new XAttribute(OpfDocument.Attributes.UniqueIdentifier, opf.UniqueIdentifier));
            }

            string versionString;
            switch (opf.EpubVersion)
            {
                case EpubVersion.Epub2:
                    versionString = "2.0";
                    break;
                case EpubVersion.Epub3:
                    versionString = "3.0";
                    break;
                default:
                    throw new EpubWriteException($"Unknown version: {opf.EpubVersion}");
            }
            root.Add(new XAttribute(OpfDocument.Attributes.Version, versionString));

            root.Add(WriteMetadata(opf.Metadata, opf.EpubVersion));
            root.Add(WriteManifest(opf.Manifest));
            root.Add(WriteSpine(opf.Spine));

            if (opf.Guide != null)
            {
                root.Add(WriteGuide(opf.Guide));
            }

            var xml = Constants.XmlDeclaration + "\n" + root;
            return xml;
        }

        private static XElement WriteMetadata(OpfMetadata metadata, EpubVersion version)
        {
            var root = new XElement(OpfElements.Metadata);

            foreach (var contributor in metadata.Contributors)
            {
                var element = new XElement(OpfElements.Contributor, contributor.Text);
                if (!string.IsNullOrWhiteSpace(contributor.AlternateScript))
                {
                    element.Add(new XAttribute(OpfMetadataCreator.Attributes.AlternateScript, contributor.AlternateScript));
                }
                if (!string.IsNullOrWhiteSpace(contributor.FileAs))
                {
                    element.Add(new XAttribute(OpfMetadataCreator.Attributes.FileAs, contributor.FileAs));
                }
                if (!string.IsNullOrWhiteSpace(contributor.Role))
                {
                    element.Add(new XAttribute(OpfMetadataCreator.Attributes.Role, contributor.Role));
                }
                root.Add(element);
            }
            foreach (var description in metadata.Descriptions)
            {
                root.Add(new XElement(OpfElements.Description, description));
            }
            foreach (var lang in metadata.Languages)
            {
                root.Add(new XElement(OpfElements.Language, lang));
            }
            foreach (var title in metadata.Titles)
            {
                root.Add(new XElement(OpfElements.Title, title));
            }
            foreach (var creator in metadata.Creators)
            {
                var element = new XElement(OpfElements.Creator, creator.Text);
                if (!string.IsNullOrWhiteSpace(creator.AlternateScript))
                {
                    element.Add(new XAttribute(OpfMetadataCreator.Attributes.AlternateScript, creator.AlternateScript));
                }
                if (!string.IsNullOrWhiteSpace(creator.FileAs))
                {
                    element.Add(new XAttribute(OpfMetadataCreator.Attributes.FileAs, creator.FileAs));
                }
                if (!string.IsNullOrWhiteSpace(creator.Role))
                {
                    element.Add(new XAttribute(OpfMetadataCreator.Attributes.Role, creator.Role));
                }
                root.Add(element);
            }
            foreach (var publisher in metadata.Publishers)
            {
                root.Add(new XElement(OpfElements.Publisher, publisher));
            }
            foreach (var subject in metadata.Subjects)
            {
                root.Add(new XElement(OpfElements.Subject, subject));
            }
            foreach (var meta in metadata.Metas)
            {
                var element = new XElement(OpfElements.Meta);

                switch (version)
                {
                    case EpubVersion.Epub2:
                        element.Add(new XAttribute(OpfMetadataMeta.Attributes.Content, meta.Text));
                        break;
                    case EpubVersion.Epub3:
                        element.Add(meta.Text);
                        break;
                    default:
                        throw new NotImplementedException($"Epub version not support: {version} ({nameof(OpfWriter)})");
                }

                if (!string.IsNullOrWhiteSpace(meta.Id))
                {
                    element.Add(new XAttribute(OpfMetadataMeta.Attributes.Id, meta.Id));
                }
                if (!string.IsNullOrWhiteSpace(meta.Name))
                {
                    element.Add(new XAttribute(OpfMetadataMeta.Attributes.Name, meta.Name));
                }
                if (!string.IsNullOrWhiteSpace(meta.Property))
                {
                    element.Add(new XAttribute(OpfMetadataMeta.Attributes.Property, meta.Property));
                }
                if (!string.IsNullOrWhiteSpace(meta.Refines))
                {
                    element.Add(new XAttribute(OpfMetadataMeta.Attributes.Refines, meta.Refines));
                }
                if (!string.IsNullOrWhiteSpace(meta.Scheme))
                {
                    element.Add(new XAttribute(OpfMetadataMeta.Attributes.Scheme, meta.Scheme));
                }
                root.Add(element);
            }
            foreach (var coverage in metadata.Coverages)
            {
                root.Add(new XElement(OpfElements.Coverages, coverage));
            }
            foreach (var date in metadata.Dates)
            {
                var element = new XElement(OpfElements.Date, date.Text);
                if (!string.IsNullOrWhiteSpace(date.Event))
                {
                    element.Add(new XAttribute(OpfMetadataDate.Attributes.Event, date.Event));
                }
                root.Add(element);
            }
            foreach (var format in metadata.Formats)
            {
                root.Add(new XElement(OpfElements.Format, format));
            }
            foreach (var identifier in metadata.Identifiers)
            {
                var element = new XElement(OpfElements.Identifier, identifier.Text);
                if (!string.IsNullOrWhiteSpace(identifier.Id))
                {
                    element.Add(new XAttribute(OpfMetadataIdentifier.Attributes.Id, identifier.Id));
                }
                if (!string.IsNullOrWhiteSpace(identifier.Scheme))
                {
                    element.Add(new XAttribute(OpfMetadataIdentifier.Attributes.Scheme, identifier.Scheme));
                }
                root.Add(element);
            }
            foreach (var relation in metadata.Relations)
            {
                root.Add(new XElement(OpfElements.Relation, relation));
            }
            foreach (var right in metadata.Rights)
            {
                root.Add(new XElement(OpfElements.Rights, right));
            }
            foreach (var source in metadata.Sources)
            {
                root.Add(new XElement(OpfElements.Source, source));
            }
            foreach (var type in metadata.Types)
            {
                root.Add(new XElement(OpfElements.Type, type));
            }

            return root;
        }

        private static XElement WriteManifest(OpfManifest manifest)
        {
            var root = new XElement(OpfElements.Manifest);

            foreach (var item in manifest.Items)
            {
                var element = new XElement(OpfElements.Item);

                if (item.Fallback != null)
                {
                    element.Add(new XAttribute(OpfManifestItem.Attributes.Fallback, item.Fallback));
                }
                if (item.FallbackStyle != null)
                {
                    element.Add(new XAttribute(OpfManifestItem.Attributes.FallbackStyle, item.FallbackStyle));
                }
                if (item.Href != null)
                {
                    element.Add(new XAttribute(OpfManifestItem.Attributes.Href, item.Href));
                }
                if (item.Id != null)
                {
                    element.Add(new XAttribute(OpfManifestItem.Attributes.Id, item.Id));
                }
                if (item.MediaType != null)
                {
                    element.Add(new XAttribute(OpfManifestItem.Attributes.MediaType, item.MediaType));
                }
                if (item.Properties.Any())
                {
                    element.Add(new XAttribute(OpfManifestItem.Attributes.Properties, string.Join(" ", item.Properties)));
                }
                if (item.RequiredModules != null)
                {
                    element.Add(new XAttribute(OpfManifestItem.Attributes.RequiredModules, item.RequiredModules));
                }
                if (item.RequiredNamespace != null)
                {
                    element.Add(new XAttribute(OpfManifestItem.Attributes.RequiredNamespace, item.RequiredNamespace));
                }

                root.Add(element);
            }

            return root;
        }

        private static XElement WriteSpine(OpfSpine spine)
        {
            var root = new XElement(OpfElements.Spine);

            if (spine.Toc != null)
            {
                root.Add(new XAttribute(OpfSpine.Attributes.Toc, spine.Toc));
            }

            foreach (var itemref in spine.ItemRefs)
            {
                var element = new XElement(OpfElements.ItemRef);
                if (itemref.Id != null)
                {
                    element.Add(new XAttribute(OpfSpineItemRef.Attributes.Id, itemref.Id));
                }
                if (itemref.IdRef != null)
                {
                    element.Add(new XAttribute(OpfSpineItemRef.Attributes.IdRef, itemref.IdRef));
                }
                if (itemref.Properties.Any())
                {
                    element.Add(new XAttribute(OpfSpineItemRef.Attributes.Properties, string.Join(" ", itemref.Properties)));
                }
                if (!itemref.Linear) // Defualt is true
                {
                    element.Add(new XAttribute(OpfSpineItemRef.Attributes.Linear, "no"));
                }
                root.Add(element);
            }

            return root;
        }

        private static XElement WriteGuide(OpfGuide guide)
        {
            var root = new XElement(OpfElements.Guide);

            foreach (var reference in guide.References)
            {
                var element = new XElement(OpfElements.Reference);
                if (!string.IsNullOrWhiteSpace(reference.Href))
                {
                    element.Add(new XAttribute(OpfGuideReference.Attributes.Href, reference.Href));
                }
                if (!string.IsNullOrWhiteSpace(reference.Title))
                {
                    element.Add(new XAttribute(OpfGuideReference.Attributes.Title, reference.Title));
                }
                if (!string.IsNullOrWhiteSpace(reference.Type))
                {
                    element.Add(new XAttribute(OpfGuideReference.Attributes.Type, reference.Type));
                }
                root.Add(element);
            }

            return root;
        }
    }
}
