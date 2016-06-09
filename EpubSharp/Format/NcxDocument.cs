using System.Collections.Generic;
using EpubSharp.Schema.Navigation;

namespace EpubSharp.Format
{
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
