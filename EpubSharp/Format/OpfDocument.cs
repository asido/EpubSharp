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
        public EpubVersion EpubVersion { get; internal set; } = new EpubVersion();
        public OpfMetadata Metadata { get; internal set; } = new OpfMetadata();
        public OpfManifest Manifest { get; internal set; } = new OpfManifest();
        public OpfSpine Spine { get; internal set; } = new OpfSpine();
        public OpfGuide Guide { get; internal set; } = new OpfGuide();

        // Below are helper properties, which aren't part of the format.
        public string NavPath { get; internal set; }
        public string NcxPath { get; internal set; }
        public string CoverPath { get; internal set; }
    }

    public class OpfMetadata
    {
        public ICollection<string> Titles { get; internal set; } = new List<string>();
        public ICollection<string> Subjects { get; internal set; } = new List<string>();
        public ICollection<string> Descriptions { get; internal set; } = new List<string>();
        public ICollection<string> Publishers { get; internal set; } = new List<string>();
        public ICollection<OpfMetadataCreator> Creators { get; internal set; } = new List<OpfMetadataCreator>();
        public ICollection<OpfMetadataCreator> Contributors { get; internal set; } = new List<OpfMetadataCreator>();
        public ICollection<OpfMetadataDate> Dates { get; internal set; } = new List<OpfMetadataDate>();
        public ICollection<string> Types { get; internal set; } = new List<string>();
        public ICollection<string> Formats { get; internal set; } = new List<string>();
        public ICollection<OpfMetadataIdentifier> Identifiers { get; internal set; } = new List<OpfMetadataIdentifier>();
        public ICollection<string> Sources { get; internal set; } = new List<string>();
        public ICollection<string> Languages { get; internal set; } = new List<string>();
        public ICollection<string> Relations { get; internal set; } = new List<string>();
        public ICollection<string> Coverages { get; internal set; } = new List<string>();
        public ICollection<string> Rights { get; internal set; } = new List<string>();
        public ICollection<OpfMetadataMeta> Metas { get; internal set; } = new List<OpfMetadataMeta>();
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
        public ICollection<OpfManifestItem> Items { get; internal set; } = new List<OpfManifestItem>();
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
        public ICollection<OpfSpineItemRef> ItemRefs { get; internal set; } = new List<OpfSpineItemRef>();
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
        public ICollection<OpfGuideReference> References { get; internal set; } = new List<OpfGuideReference>();
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
