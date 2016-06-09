using System.Collections.Generic;
using EpubSharp.Schema.Opf;

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
        public EpubMetadata Metadata { get; internal set; }
        public EpubManifest Manifest { get; internal set; }
        public EpubSpine Spine { get; internal set; }
        public EpubGuide Guide { get; internal set; }
    }

    public class EpubMetadata
    {
        public IReadOnlyCollection<string> Titles { get; internal set; }
        public IReadOnlyCollection<EpubMetadataCreator> Creators { get; internal set; }
        public IReadOnlyCollection<string> Subjects { get; internal set; }
        public string Description { get; internal set; }
        public IReadOnlyCollection<string> Publishers { get; internal set; }
        public IReadOnlyCollection<EpubMetadataCreator> Contributors { get; internal set; }
        public string Date { get; internal set; }
        public IReadOnlyCollection<string> Types { get; internal set; }
        public IReadOnlyCollection<string> Formats { get; internal set; }
        public IReadOnlyCollection<EpubMetadataIdentifier> Identifiers { get; internal set; }
        public IReadOnlyCollection<string> Sources { get; internal set; }
        public IReadOnlyCollection<string> Languages { get; internal set; }
        public IReadOnlyCollection<string> Relations { get; internal set; }
        public IReadOnlyCollection<string> Coverages { get; internal set; }
        public IReadOnlyCollection<string> Rights { get; internal set; }
        public IReadOnlyCollection<EpubMetadataMeta> MetaItems { get; internal set; }
    }

    public class EpubMetadataCreator
    {
        public string Text { get; internal set; }
        public string Role { get; internal set; }
        public string FileAs { get; internal set; }
        public string AlternateScript { get; internal set; }
    }

    public class EpubMetadataIdentifier
    {
        public string Id { get; internal set; }
        public string Scheme { get; internal set; }
        public string Text { get; internal set; }
    }

    public class EpubMetadataMeta
    {
        public string Name { get; internal set; }
        public string Content { get; internal set; }
        public string Id { get; internal set; }
        public string Refines { get; internal set; }
        public string Property { get; internal set; }
        public string Scheme { get; internal set; }
    }
}
