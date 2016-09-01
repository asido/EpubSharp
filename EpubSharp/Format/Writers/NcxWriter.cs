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
