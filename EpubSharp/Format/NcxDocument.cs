using System.Collections.Generic;
using EpubSharp.Schema.Navigation;

namespace EpubSharp.Format
{
    /// <summary>
    /// DAISY’s Navigation Center eXtended (NCX)
    /// </summary>
    public class NcxDocument
    {
        public IReadOnlyCollection<EpubNcxMetadata> Metadata { get; internal set; }
        public IReadOnlyCollection<string> DocTitle { get; internal set; }
        public IReadOnlyCollection<string> DocAuthors { get; internal set; }
        public IReadOnlyCollection<EpubNcxNavigationPoint> NavMap { get; internal set; }
        public IReadOnlyCollection<EpubNavigationPageTarget> PageList { get; internal set; }
        public IReadOnlyCollection<EpubNavigationList> NavLists { get; internal set; }
    }

    public class EpubNcxMetadata
    {
        public string Name { get; internal set; }
        public string Content { get; internal set; }
        public string Scheme { get; internal set; }
    }

    public class EpubNcxNavigationPoint
    {
        public string Id { get; internal set; }
        public string Class { get; internal set; }
        public string PlayOrder { get; internal set; }
        public IReadOnlyCollection<string> NavigationLabels { get; internal set; }
        public string ContentSource { get; internal set; }
        public List<EpubNcxNavigationPoint> ChildNavigationPoints { get; internal set; }

        public override string ToString()
        {
            return $"Id: {Id}, ContentSource: {ContentSource}";
        }
    }
}
