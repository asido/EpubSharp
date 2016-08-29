using System;
using System.Linq;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class OcfReader
    {
        private static class OcfElements
        {
            public static readonly XName RootFile = Constants.OcfNamespace + "rootfile";
        }

        public static OcfDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            var element = xml.Root.Descendants(OcfElements.RootFile).FirstOrDefault();
            var rootFile = element?.Attribute("full-path")?.Value;
            if (string.IsNullOrWhiteSpace(rootFile))
            {
                throw new EpubParseException("Malformed OCF.");
            }
            return new OcfDocument { RootFile = rootFile };
        }
    }
}
