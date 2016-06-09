using EpubSharp.Schema.Navigation;
using EpubSharp.Schema.Opf;

namespace EpubSharp.Format
{
    public class EpubFormat
    {
        public PackageDocument PackageDocument { get; internal set; }
    }

    public class PackageDocument
    {
        public EpubPackage Package { get; set; }
        public EpubNavigation Navigation { get; set; }
        public string ContentDirectoryPath { get; set; }
    }
}
