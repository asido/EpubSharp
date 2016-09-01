using System;
using System.Linq;
using System.Text;
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

            var metadata = new XElement(OpfElements.Metadata);
            foreach (var lang in opf.Metadata.Languages)
            {
                metadata.Add(new XElement(OpfElements.Language, lang));
            }
            foreach (var title in opf.Metadata.Titles)
            {
                metadata.Add(new XElement(OpfElements.Title, title));
            }
            foreach (var creator in opf.Metadata.Creators)
            {
                metadata.Add(new XElement(OpfElements.Creator, creator));
            }
            foreach (var publisher in opf.Metadata.Publishers)
            {
                metadata.Add(new XElement(OpfElements.Publisher, publisher));
            }
            foreach (var subject in opf.Metadata.Subjects)
            {
                metadata.Add(new XElement(OpfElements.Subject, subject));
            }
            metadata.Add(new XElement(OpfElements.Date, DateTimeOffset.UtcNow));
            root.Add(metadata);

            var manifest = new XElement(OpfElements.Manifest);
            foreach (var item in opf.Manifest.Items)
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
                manifest.Add(element);
            }
            root.Add(manifest);

            var spine = new XElement(OpfElements.Spine);
            if (opf.Spine.Toc != null)
            {
                spine.Add(new XAttribute(OpfSpine.Attributes.Toc, opf.Spine.Toc));
            }
            foreach (var itemref in opf.Spine.ItemRefs)
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
                if (itemref.Properties != null && itemref.Properties.Length > 0)
                {
                    element.Add(new XAttribute(OpfSpineItemRef.Attributes.Properties, string.Join(" ", itemref.Properties)));
                }
                if (!itemref.Linear) // Defualt is true
                {
                    element.Add(new XAttribute(OpfSpineItemRef.Attributes.Linear, "no"));
                }
                spine.Add(element);
            }
            root.Add(spine);

            var xml = Constants.XmlDeclaration + "\n" + root;
            return xml;
        }
    }
}
