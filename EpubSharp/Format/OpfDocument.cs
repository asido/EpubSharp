using System.Collections.Generic;

namespace EpubSharp.Format
{
    public enum EpubVersion
    {
        Epub2 = 2,
        Epub3
    }
    
    public class OpfDocument
    {
        public EpubVersion EpubVersion { get; internal set; }
        public OpfMetadata Metadata { get; internal set; }
        public OpfManifest Manifest { get; internal set; }
        public OpfSpine Spine { get; internal set; }
        public OpfGuide Guide { get; internal set; }

        // Below are helper properties, which aren't part of the format.
        public string NavPath { get; internal set; }
        public string NcxPath { get; internal set; }
        public string CoverPath { get; internal set; }
    }

    public class OpfMetadata
    {
        public IReadOnlyCollection<string> Titles { get; internal set; }
        public IReadOnlyCollection<string> Subjects { get; internal set; }
        public IReadOnlyCollection<string> Descriptions { get; internal set; }
        public IReadOnlyCollection<string> Publishers { get; internal set; }
        public IReadOnlyCollection<OpfMetadataCreator> Creators { get; internal set; }
        public IReadOnlyCollection<OpfMetadataCreator> Contributors { get; internal set; }
        public IReadOnlyCollection<OpfMetadataDate> Dates { get; internal set; }
        public IReadOnlyCollection<string> Types { get; internal set; }
        public IReadOnlyCollection<string> Formats { get; internal set; }
        public IReadOnlyCollection<OpfMetadataIdentifier> Identifiers { get; internal set; }
        public IReadOnlyCollection<string> Sources { get; internal set; }
        public IReadOnlyCollection<string> Languages { get; internal set; }
        public IReadOnlyCollection<string> Relations { get; internal set; }
        public IReadOnlyCollection<string> Coverages { get; internal set; }
        public IReadOnlyCollection<string> Rights { get; internal set; }
        public IReadOnlyCollection<OpfMetadataMeta> Metas { get; internal set; }
    }

    public class OpfMetadataDate
    {
        public string Text { get; internal set; }

        /// <summary>
        /// i.e. "modification"
        /// </summary>
        public string Event { get; internal set; }
    }

    public class OpfMetadataCreator
    {
        public string Text { get; internal set; }
        public string Role { get; internal set; }
        public string FileAs { get; internal set; }
        public string AlternateScript { get; internal set; }
    }

    public class OpfMetadataIdentifier
    {
        public string Id { get; internal set; }
        public string Scheme { get; internal set; }
        public string Text { get; internal set; }
    }

    public class OpfMetadataMeta
    {
        public string Name { get; internal set; }
        public string Id { get; internal set; }
        public string Refines { get; internal set; }
        public string Property { get; internal set; }
        public string Scheme { get; internal set; }
        public string Text { get; internal set; }
    }

    public class OpfManifest
    {
        public IReadOnlyCollection<OpfManifestItem> Items { get; internal set; }
    }

    public class OpfManifestItem
    {
        public string Id { get; internal set; }
        public string Href { get; internal set; }
        public ICollection<string> Properties { get; internal set; } = new List<string>();
        public string MediaType { get; internal set; }
        public string RequiredNamespace { get; internal set; }
        public string RequiredModules { get; internal set; }
        public string Fallback { get; internal set; }
        public string FallbackStyle { get; internal set; }

        public override string ToString()
        {
            return $"Id: {Id}, Href = {Href}, MediaType = {MediaType}";
        }
    }

    public class OpfSpine
    {
        public string Toc { get; internal set; }
        public IReadOnlyCollection<OpfSpineItemRef> ItemRefs { get; internal set; }
    }

    public class OpfSpineItemRef
    {
        public string IdRef { get; internal set; }
        public bool Linear { get; internal set; }
        public string Id { get; internal set; }
        public string[] Properties { get; internal set; }

        public override string ToString()
        {
            return "IdRef: " + IdRef;
        }
    }

    public class OpfGuide
    {
        public IReadOnlyCollection<OpfGuideReference> References { get; internal set; }
    }

    public class OpfGuideReference
    {
        public string Type { get; internal set; }
        public string Title { get; internal set; }
        public string Href { get; internal set; }

        public override string ToString()
        {
            return $"Type: {Type}, Href: {Href}";
        }
    }
}
