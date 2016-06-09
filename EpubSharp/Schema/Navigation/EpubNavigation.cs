using System.Collections.Generic;

namespace EpubSharp.Schema.Navigation
{
    public class EpubNavigation
    {
        public IReadOnlyCollection<EpubNavigationHeadMeta> Head { get; set; }
        public IReadOnlyCollection<string> DocTitle { get; set; }
        public IReadOnlyCollection<string> DocAuthors { get; set; }
        public IReadOnlyCollection<EpubNavigationPoint> NavMap { get; set; }
        public EpubNavigationPageList PageList { get; set; }
        public List<EpubNavigationList> NavLists { get; set; }
    }
}
