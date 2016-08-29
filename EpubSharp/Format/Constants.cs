using System.Xml.Linq;

namespace EpubSharp.Format
{
    internal class Constants
    {
        public const string OcfPath = "META-INF/container.xml";

        public static readonly XNamespace OcfNamespace = "urn:oasis:names:tc:opendocument:xmlns:container";
        public static readonly XNamespace NcxNamespace = "http://www.daisy.org/z3986/2005/ncx/";
        public static readonly XNamespace OpfNamespace = "http://www.idpf.org/2007/opf";
        public static readonly XNamespace OpfMetadataNamespace = "http://purl.org/dc/elements/1.1/";
    }
}
