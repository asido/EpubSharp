using System.Text;
using System.Xml.Linq;

namespace EpubSharp.Format
{
    internal class Constants
    {
        public static readonly string Html5Doctype = "<!DOCTYPE html>";

        public static readonly string XmlDeclarationPrefix = "<?xml";
        public static readonly string XmlDeclarationSufix = "?>";

        public static readonly string XmlDeclaration = new XDeclaration("1.0", null, null).ToString();

        public static readonly XNamespace XhtmlNamespace = "http://www.w3.org/1999/xhtml";
        public static readonly XNamespace OcfNamespace = "urn:oasis:names:tc:opendocument:xmlns:container";
        public static readonly XNamespace NcxNamespace = "http://www.daisy.org/z3986/2005/ncx/";
        public static readonly XNamespace OpfNamespace = "http://www.idpf.org/2007/opf";
        public static readonly XNamespace OpfMetadataNamespace = "http://purl.org/dc/elements/1.1/";
        public static readonly XNamespace OpsNamespace = "http://www.idpf.org/2007/ops";

        public const string OcfPath = "META-INF/container.xml";
        public const string OcfMediaType = "application/oebps-package+xml";

        public const string DefaultOpfUniqueIdentifier = "uuid_id";

        public static readonly Encoding DefaultEncoding = Encoding.UTF8;
    }
}
