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
            var root = new XElement(Constants.OpfNamespace + "package");
            root.Add(new XAttribute("xmlns", Constants.OpfNamespace));
            root.Add(new XAttribute(XNamespace.Xmlns + "dc", Constants.OpfMetadataNamespace));

            root.Add(new XAttribute("version", "2.0"));

            var metadata = new XElement(Constants.OpfNamespace + "metadata");
            foreach (var lang in opf.Metadata.Languages)
            {
                metadata.Add(new XElement(Constants.OpfMetadataNamespace + "language", lang));
            }
            foreach (var title in opf.Metadata.Titles)
            {
                metadata.Add(new XElement(Constants.OpfMetadataNamespace + "title", title));
            }
            foreach (var creator in opf.Metadata.Creators)
            {
                metadata.Add(new XElement(Constants.OpfMetadataNamespace + "creator", creator));
            }
            foreach (var publisher in opf.Metadata.Publishers)
            {
                metadata.Add(new XElement(Constants.OpfMetadataNamespace + "publisher", publisher));
            }
            foreach (var subject in opf.Metadata.Subjects)
            {
                metadata.Add(new XElement(Constants.OpfMetadataNamespace + "subject", subject));
            }
            metadata.Add(new XElement(Constants.OpfMetadataNamespace + "date", DateTimeOffset.UtcNow));
            root.Add(metadata);

            var manifest = new XElement(Constants.OpfNamespace + "manifest");
            if (opf.CoverPath != null)
            {
                var cover = opf.Manifest.Items.FirstOrDefault(e => e.Href == opf.CoverPath);
                if (cover == null)
                {
                    throw new EpubWriteException($"Cover path is set to '{opf.CoverPath}', but couldn't find any manifest item with such href.");
                }
                manifest.Add(new XElement(Constants.OpfNamespace + "item", new XAttribute("id", "cover-image"), new XAttribute("href", cover.Href), new XAttribute("media-type", cover.MediaType), new XAttribute("properties", "cover-image")));
            }
            if (opf.Spine.Toc != null)
            {
                if (opf.NcxPath == null)
                {
                    throw new EpubWriteException("Spine TOC is set, but NCX path is not.");
                }
                manifest.Add(new XElement(Constants.OpfNamespace + "item", new XAttribute("id", "ncx"), new XAttribute("media-type", ContentType.ContentTypeToMimeType[EpubContentType.DtbookNcx]), new XAttribute("href", opf.NcxPath)));
            }
            foreach (var item in opf.Manifest.Items)
            {
                var element = new XElement(Constants.OpfNamespace + "item");
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

            var spine = new XElement(Constants.OpfNamespace + "spine");
            if (opf.Spine.Toc != null)
            {
                spine.Add(new XAttribute("toc", opf.Spine.Toc));
            }
            foreach (var itemref in opf.Spine.ItemRefs)
            {
                var element = new XElement(Constants.OpfNamespace + "itemref");
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
            spine.Add(new XElement(Constants.OpfNamespace + "itemref", new XAttribute("idref", "BookCover")));
            root.Add(spine);

            return root.ToString();
        }
    }
}
