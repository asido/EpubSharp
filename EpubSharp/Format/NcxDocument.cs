using System.Collections.Generic;

namespace EpubSharp.Format
{
    /// <summary>
    /// DAISY’s Navigation Center eXtended (NCX)
    /// </summary>
    public class NcxDocument
    {
        public IReadOnlyCollection<NcxMetadata> Metadata { get; internal set; }
        public string DocTitle { get; internal set; }
        public string DocAuthor { get; internal set; }
        public IReadOnlyCollection<NcxNavigationPoint> NavigationMap { get; internal set; }
        public IReadOnlyCollection<NcxPageTarget> PageList { get; internal set; }
        public NcxNavigationList NavigationList { get; internal set; }
    }

    public class NcxMetadata
    {
        public string Name { get; internal set; }
        public string Content { get; internal set; }
        public string Scheme { get; internal set; }
    }

    public class NcxNavigationPoint
    {
        public string Id { get; internal set; }
        public string Class { get; internal set; }
        public int? PlayOrder { get; internal set; }
        public string LabelText { get; internal set; }
        public string ContentSrc { get; internal set; }
        public IReadOnlyCollection<NcxNavigationPoint> NavigationPoints { get; internal set; }

        public override string ToString()
        {
            return $"Id: {Id}, ContentSource: {ContentSrc}";
        }
    }

    public enum NcxPageTargetType
    {
        Front = 1,
        Normal,
        Special,
        Body
    }

    public class NcxPageTarget
    {
        public string Id { get; internal set; }
        public int? Value { get; internal set; }
        public string Class { get; internal set; }
        public NcxPageTargetType? Type { get; internal set; }
        public string Label { get; internal set; }
        public string ContentSource { get; internal set; }
    }

    public class NcxNavigationList
    {
        public string Id { get; internal set; }
        public string Class { get; internal set; }
        public string Label { get; internal set; }
        public IReadOnlyCollection<NcxNavigationTarget> NavigationTargets { get; internal set; }
    }

    public class NcxNavigationTarget
    {
        public string Id { get; internal set; }
        public string Class { get; internal set; }
        public int? PlayOrder { get; internal set; }
        public string Label { get; internal set; }
        public string ContentSource { get; internal set; }
    }
}
