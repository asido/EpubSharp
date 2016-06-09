using System.Collections.Generic;
using EpubSharp.Schema.Navigation;
using EpubSharp.Schema.Opf;

namespace EpubSharp.Format
{
    public class EpubFormat
    {
        public OcfDocument Ocf { get; internal set; }
        public PackageDocument Package { get; internal set; }
        public NcxDocument Ncx { get; internal set; }
    }

    public class OcfDocument
    {
        public string RootFile { get; internal set; }
    }
    
    public class NcxDocument
    {
        public IReadOnlyCollection<EpubNavigationHeadMeta> Head { get; internal set; }
        public IReadOnlyCollection<string> DocTitle { get; internal set; }
        public IReadOnlyCollection<string> DocAuthors { get; internal set; }
        public IReadOnlyCollection<EpubNavigationPoint> NavMap { get; internal set; }
        public IReadOnlyCollection<EpubNavigationPageTarget> PageList { get; internal set; }
        public IReadOnlyCollection<EpubNavigationList> NavLists { get; internal set; }
    }
}
