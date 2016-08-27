using System.Collections.Generic;

namespace EpubSharp.Format
{
    public enum EpubVersion
    {
        Epub2 = 2,
        Epub3
    }
    
    public class PackageDocument
    {
        public EpubVersion EpubVersion { get; internal set; }
        public PackageMetadata Metadata { get; internal set; }
        public PackageManifest Manifest { get; internal set; }
        public PackageSpine Spine { get; internal set; }
        public PackageGuide Guide { get; internal set; }

        // Below are helper properties, which aren't part of the format.
        public string NavPath { get; internal set; }
        public string NcxPath { get; internal set; }
        public string CoverPath { get; internal set; }
    }

    public class PackageMetadata
    {
        public IReadOnlyCollection<string> Titles { get; internal set; }
        public IReadOnlyCollection<string> Subjects { get; internal set; }
        public IReadOnlyCollection<string> Descriptions { get; internal set; }
        public IReadOnlyCollection<string> Publishers { get; internal set; }
        public IReadOnlyCollection<PackageMetadataCreator> Creators { get; internal set; }
        public IReadOnlyCollection<PackageMetadataCreator> Contributors { get; internal set; }
        public IReadOnlyCollection<PackageMetadataDate> Dates { get; internal set; }
        public IReadOnlyCollection<string> Types { get; internal set; }
        public IReadOnlyCollection<string> Formats { get; internal set; }
        public IReadOnlyCollection<PackageMetadataIdentifier> Identifiers { get; internal set; }
        public IReadOnlyCollection<string> Sources { get; internal set; }
        public IReadOnlyCollection<string> Languages { get; internal set; }
        public IReadOnlyCollection<string> Relations { get; internal set; }
        public IReadOnlyCollection<string> Coverages { get; internal set; }
        public IReadOnlyCollection<string> Rights { get; internal set; }
        public IReadOnlyCollection<PackageMetadataMeta> MetaItems { get; internal set; }
    }

    public class PackageMetadataDate
    {
        public string Text { get; internal set; }

        /// <summary>
        /// i.e. "modification"
        /// </summary>
        public string Event { get; internal set; }
    }

    public class PackageMetadataCreator
    {
        public string Text { get; internal set; }
        public string Role { get; internal set; }
        public string FileAs { get; internal set; }
        public string AlternateScript { get; internal set; }
    }

    public class PackageMetadataIdentifier
    {
        public string Id { get; internal set; }
        public string Scheme { get; internal set; }
        public string Text { get; internal set; }
    }

    public class PackageMetadataMeta
    {
        public string Name { get; internal set; }
        public string Content { get; internal set; }
        public string Id { get; internal set; }
        public string Refines { get; internal set; }
        public string Property { get; internal set; }
        public string Scheme { get; internal set; }
    }

    public class PackageManifest
    {
        public IReadOnlyCollection<PackageManifestItem> Items { get; internal set; }
    }

    public class PackageManifestItem
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

    public class PackageSpine
    {
        public string Toc { get; internal set; }
        public IReadOnlyCollection<PackageSpineItemRef> ItemRefs { get; internal set; }
    }

    public class PackageSpineItemRef
    {
        public string IdRef { get; internal set; }
        public bool IsLinear { get; internal set; }

        public override string ToString()
        {
            return "IdRef: " + IdRef;
        }
    }

    public class PackageGuide
    {
        public IReadOnlyCollection<PackageGuideReference> References { get; internal set; }
    }

    public class PackageGuideReference
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
