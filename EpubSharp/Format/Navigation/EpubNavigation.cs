using System.Collections.Generic;

namespace EpubSharp.Schema.Navigation
{
    public class EpubNavigation
    {
        public IReadOnlyCollection<EpubNavigationHeadMeta> Head { get; set; }
        public IReadOnlyCollection<string> DocTitle { get; set; }
        public IReadOnlyCollection<string> DocAuthors { get; set; }
        public IReadOnlyCollection<EpubNavigationPoint> NavMap { get; set; }
        public IReadOnlyCollection<EpubNavigationPageTarget> PageList { get; set; }
        public IReadOnlyCollection<EpubNavigationList> NavLists { get; set; }
    }
}
