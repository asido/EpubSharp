using System.Collections.Generic;

namespace EpubSharp.Format
{
    /// <summary>
    /// DAISY’s Navigation Center eXtended (NCX)
    /// </summary>
    public class NcxDocument
    {
        public IReadOnlyCollection<EpubNcxMetadata> Metadata { get; internal set; }
        public string DocTitle { get; internal set; }
        public string DocAuthor { get; internal set; }
        public IReadOnlyCollection<EpubNcxNavigationPoint> NavigationMap { get; internal set; }
        public IReadOnlyCollection<EpubNcxPageTarget> PageList { get; internal set; }
        public EpubNcxNavigationList NavigationList { get; internal set; }
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
        public int? PlayOrder { get; internal set; }
        public string LabelText { get; internal set; }
        public string ContentSrc { get; internal set; }
        public IReadOnlyCollection<EpubNcxNavigationPoint> NavigationPoints { get; internal set; }

        public override string ToString()
        {
            return $"Id: {Id}, ContentSource: {ContentSrc}";
        }
    }

    public enum EpubNcxPageTargetType
    {
        Front = 1,
        Normal,
        Special,
        Body
    }

    public class EpubNcxPageTarget
    {
        public string Id { get; internal set; }
        public int? Value { get; internal set; }
        public string Class { get; internal set; }
        public EpubNcxPageTargetType? Type { get; internal set; }
        public string Label { get; internal set; }
        public string ContentSource { get; internal set; }
    }

    public class EpubNcxNavigationList
    {
        public string Id { get; internal set; }
        public string Class { get; internal set; }
        public string Label { get; internal set; }
        public IReadOnlyCollection<EpubNcxNavigationTarget> NavigationTargets { get; internal set; }
    }

    public class EpubNcxNavigationTarget
    {
        public string Id { get; internal set; }
        public string Class { get; internal set; }
        public int? PlayOrder { get; internal set; }
        public string Label { get; internal set; }
        public string ContentSource { get; internal set; }
    }
}
