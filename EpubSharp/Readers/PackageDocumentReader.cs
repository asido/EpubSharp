using System.Xml;
using EpubSharp.Format;

namespace EpubSharp.Readers
{
    internal static class PackageDocumentReader
    {
        public static PackageDocument Read(XmlDocument xml)
        {
            return PackageReader.ReadPackage(xml);
        }
    }
}
