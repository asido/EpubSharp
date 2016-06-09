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
        public EpubVersion EpubVersion { get; set; }
        public EpubMetadata Metadata { get; set; }
        public EpubManifest Manifest { get; set; }
        public EpubSpine Spine { get; set; }
        public EpubGuide Guide { get; set; }

        public EpubNavigation Navigation { get; set; }
        public string ContentDirectoryPath { get; set; }
    }
}
