using System;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class OcfReader
    {
        public static OcfDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            var rootFiles = xml.Root?.Element(OcfElements.RootFiles)?.Elements(OcfElements.RootFile);
            var ocf = new OcfDocument
            {
                RootFiles = rootFiles.AsObjectList(elem => new OcfRootFile
                {
                    FullPath = (string) elem.Attribute(OcfRootFile.Attributes.FullPath),
                    MediaType = (string) elem.Attribute(OcfRootFile.Attributes.MediaType)
                })
            };
            return ocf;
        }
    }
}
