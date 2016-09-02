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
                if (!string.IsNullOrWhiteSpace(meta.Content))
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

            var navMap = new XElement(NcxElements.NavMap);
            WriteNavPoints(navMap, ncx.NavMap.NavPoints);
            root.Add(navMap);

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
