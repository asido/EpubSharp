using System.Collections.Generic;

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
        public IReadOnlyCollection<EpubNcxNavigationPoint> NavigationMap { get; internal set; }
        public IReadOnlyCollection<EpubNcxPageTarget> PageList { get; internal set; }
        public IReadOnlyCollection<EpubNcxNavigationList> NavigationList { get; internal set; }
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
        public List<EpubNcxNavigationPoint> NavigationPoints { get; internal set; }

        public override string ToString()
        {
            return $"Id: {Id}, ContentSource: {ContentSource}";
        }
    }

    public enum EpubNcxPageTargetType
    {
        Front = 1,
        Normal,
        Special
    }

    public class EpubNcxPageTarget
    {
        public string Id { get; internal set; }
        public string Value { get; internal set; }
        public string Class { get; internal set; }
        public string PlayOrder { get; internal set; }
        public EpubNcxPageTargetType Type { get; internal set; }
        public IReadOnlyCollection<string> NavigationLabels { get; internal set; }
        public string ContentSource { get; internal set; }
    }

    public class EpubNcxNavigationList
    {
        public string Id { get; internal set; }
        public string Class { get; internal set; }
        public IReadOnlyCollection<string> NavigationLabels { get; internal set; }
        public List<EpubNcxNavigationTarget> NavigationTargets { get; internal set; }
    }

    public class EpubNcxNavigationTarget
    {
        public string Id { get; internal set; }
        public string Class { get; internal set; }
        public string Value { get; internal set; }
        public string PlayOrder { get; internal set; }
        public IReadOnlyCollection<string> NavigationLabels { get; internal set; }
        public string ContentSource { get; internal set; }
    }
}
