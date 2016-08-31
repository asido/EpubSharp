using System;
using System.Xml.Linq;

namespace EpubSharp.Format.Writers
{
    internal class OcfWriter
    {
        public static string Format(string opfPath)
        {
            if (string.IsNullOrWhiteSpace(opfPath)) throw new ArgumentNullException(nameof(opfPath));

            var container = new XElement(OcfElements.Container);
            container.Add(new XAttribute("xmlns", Constants.OcfNamespace));
            container.Add(new XAttribute("version", "1.0"));

            var rootfiles = new XElement(OcfElements.RootFiles);
            rootfiles.Add(new XElement(OcfElements.RootFile, new XAttribute(OcfRootFile.Attributes.FullPath, opfPath), new XAttribute(OcfRootFile.Attributes.MediaType, Constants.OcfMediaType)));

            container.Add(rootfiles);

            var xml = Constants.XmlDeclaration + "\n" + container;
            return xml;
        }
    }
}
