using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EpubSharp.Format.Writers
{
    internal class NcxWriter
    {
        public static string Format(NcxDocument ncx)
        {
            if (ncx == null) throw new ArgumentNullException(nameof(ncx));

            var root = new XElement(NcxElements.Ncx);
            root.Add(new XAttribute("xmlns", Constants.NcxNamespace));

            var head = new XElement(NcxElements.Head);
            foreach (var meta in ncx.Meta)
            {
                var element = new XElement(NcxElements.Meta);
                if (meta.Content != null)
                {
                    element.Add(new XAttribute(NcxMeta.Attributes.Content, meta.Content));
                }
                if (!string.IsNullOrWhiteSpace(meta.Name))
                {
                    element.Add(new XAttribute(NcxMeta.Attributes.Name, meta.Name));
                }
                if (!string.IsNullOrWhiteSpace(meta.Scheme))
                {
                    element.Add(new XAttribute(NcxMeta.Attributes.Scheme, meta.Scheme));
                }
                head.Add(element);
            }
            root.Add(head);

            if (!string.IsNullOrWhiteSpace(ncx.DocTitle))
            {
                root.Add(new XElement(NcxElements.DocTitle, new XElement(NcxElements.Text, ncx.DocTitle)));
            }

            // Null check instead of string.IsNullOrWhiteSpace(), because I've seen epubs having <docAuthor><text/></docAuthor>
            if (ncx.DocAuthor != null)
            {
                root.Add(new XElement(NcxElements.DocAuthor, new XElement(NcxElements.Text, ncx.DocAuthor)));
            }

            var navMap = new XElement(NcxElements.NavMap);
            WriteNavPoints(navMap, ncx.NavMap.NavPoints);
            root.Add(navMap);

            if (ncx.PageList != null)
            {
                var pageListElement = new XElement(NcxElements.PageList);

                if (ncx.PageList.NavInfo != null)
                {
                    pageListElement.Add(new XElement(NcxElements.NavInfo, new XElement(NcxElements.Text, ncx.PageList.NavInfo.Text)));
                }

                foreach (var pageTarget in ncx.PageList.PageTargets)
                {
                    var pageTargetElement = new XElement(NcxElements.PageTarget);
                    if (!string.IsNullOrWhiteSpace(pageTarget.Class))
                    {
                        pageTargetElement.Add(new XAttribute(NcxPageTarget.Attributes.Class, pageTarget.Class));
                    }
                    if (!string.IsNullOrWhiteSpace(pageTarget.Id))
                    {
                        pageTargetElement.Add(new XAttribute(NcxPageTarget.Attributes.Id, pageTarget.Id));
                    }
                    if (pageTarget.Type.HasValue)
                    {
                        pageTargetElement.Add(new XAttribute(NcxPageTarget.Attributes.Type, pageTarget.Type.Value));
                    }
                    if (!string.IsNullOrWhiteSpace(pageTarget.Value))
                    {
                        pageTargetElement.Add(new XAttribute(NcxPageTarget.Attributes.Value, pageTarget.Value));
                    }
                    if (!string.IsNullOrWhiteSpace(pageTarget.NavLabelText))
                    {
                        pageTargetElement.Add(new XElement(NcxElements.NavLabel, new XElement(NcxElements.Text, pageTarget.NavLabelText)));
                    }
                    if (!string.IsNullOrWhiteSpace(pageTarget.ContentSrc))
                    {
                        pageTargetElement.Add(new XElement(NcxElements.Content, new XAttribute(NcxPageTarget.Attributes.ContentSrc, pageTarget.ContentSrc)));
                    }
                    pageListElement.Add(pageTargetElement);
                }

                root.Add(pageListElement);
            }

            var xml = Constants.XmlDeclaration + "\n" + root;
            return xml;
        }

        private static void WriteNavPoints(XElement root, IEnumerable<NcxNavPoint> navPoints)
        {
            foreach (var navPoint in navPoints)
            {
                var element = new XElement(NcxElements.NavPoint, new XAttribute(NcxNavPoint.Attributes.Id, navPoint.Id));
                if (!string.IsNullOrWhiteSpace(navPoint.Class))
                {
                    element.Add(new XAttribute(NcxNavPoint.Attributes.Class, navPoint.Class));
                }
                if (navPoint.PlayOrder.HasValue)
                {
                    element.Add(new XAttribute(NcxNavPoint.Attributes.PlayOrder, navPoint.PlayOrder.Value));
                }
                element.Add(new XElement(NcxElements.NavLabel, new XElement(NcxElements.Text, navPoint.NavLabelText)));
                element.Add(new XElement(NcxElements.Content, new XAttribute(NcxNavPoint.Attributes.ContentSrc, navPoint.ContentSrc)));
                root.Add(element);

                if (navPoint.NavPoints.Any())
                {
                    WriteNavPoints(element, navPoint.NavPoints);
                }
            }
        }
    }
}
