using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            string versionString = null;
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
            root.Add(new XAttribute("version", versionString));

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
            var coverPath = opf.FindCoverPath();
            if (coverPath != null)
            {
                var cover = opf.Manifest.Items.FirstOrDefault(e => e.Href == coverPath);
                if (cover == null)
                {
                    throw new EpubWriteException($"Cover path is set to '{coverPath}', but couldn't find any manifest item with such href.");
                }
                manifest.Add(new XElement(OpfElements.Item, new XAttribute("id", "cover-image"), new XAttribute("href", cover.Href), new XAttribute("media-type", cover.MediaType), new XAttribute("properties", "cover-image")));
            }
            if (opf.Spine.Toc != null)
            {
                var ncxPath = opf.FindNcxPath();
                if (ncxPath == null)
                {
                    throw new EpubWriteException("Spine TOC is set, but NCX path is not.");
                }
                manifest.Add(new XElement(OpfElements.Item, new XAttribute("id", "ncx"), new XAttribute("media-type", ContentType.ContentTypeToMimeType[EpubContentType.DtbookNcx]), new XAttribute("href", ncxPath)));
            }
            foreach (var item in opf.Manifest.Items)
            {
                var element = new XElement(OpfElements.Item);
                if (item.Fallback != null)
                {
                    element.Add(new XAttribute("fallback", item.Fallback));
                }
                if (item.FallbackStyle != null)
                {
                    element.Add(new XAttribute("fallback-style", item.FallbackStyle));
                }
                if (item.Href != null)
                {
                    element.Add(new XAttribute("href", item.Href));
                }
                if (item.Id != null)
                {
                    element.Add(new XAttribute("id", item.Id));
                }
                if (item.MediaType != null)
                {
                    element.Add(new XAttribute("media-type", item.MediaType));
                }
                if (item.Properties != null)
                {
                    element.Add(new XAttribute("properties", string.Join(" ", item.Properties)));
                }
                if (item.RequiredModules != null)
                {
                    element.Add(new XAttribute("required-modules", item.RequiredModules));
                }
                if (item.RequiredNamespace != null)
                {
                    element.Add(new XAttribute("required-namespace", item.RequiredNamespace));
                }
                manifest.Add(element);
            }
            root.Add(manifest);

            var spine = new XElement(OpfElements.Spine);
            if (opf.Spine.Toc != null)
            {
                spine.Add(new XAttribute("toc", opf.Spine.Toc));
            }
            foreach (var itemref in opf.Spine.ItemRefs)
            {
                var element = new XElement(OpfElements.ItemRef);
                if (itemref.Id != null)
                {
                    element.Add(new XAttribute("id", itemref.Id));
                }
                if (itemref.IdRef != null)
                {
                    element.Add(new XAttribute("idref", itemref.IdRef));
                }
                if (itemref.Properties != null && itemref.Properties.Length > 0)
                {
                    element.Add(new XAttribute("properties", string.Join(" ", itemref.Properties)));
                }
                element.Add(new XAttribute("linear", itemref.Linear ? "yes" : "no"));
                spine.Add(element);
            }
            root.Add(spine);

            return root.ToString();
        }
    }
}
