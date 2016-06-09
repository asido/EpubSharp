using System.Collections.Generic;
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

        public NcxDocument Ncx { get; set; }
        public string ContentDirectoryPath { get; set; }
    }

    public class NcxDocument
    {
        public IReadOnlyCollection<EpubNavigationHeadMeta> Head { get; set; }
        public IReadOnlyCollection<string> DocTitle { get; set; }
        public IReadOnlyCollection<string> DocAuthors { get; set; }
        public IReadOnlyCollection<EpubNavigationPoint> NavMap { get; set; }
        public IReadOnlyCollection<EpubNavigationPageTarget> PageList { get; set; }
        public IReadOnlyCollection<EpubNavigationList> NavLists { get; set; }
    }
}
