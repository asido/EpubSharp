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
        public IReadOnlyCollection<string> Titles { get; set; }
        public IReadOnlyCollection<EpubCreator> Creators { get; set; }
        public IReadOnlyCollection<string> Subjects { get; set; }
        public string Description { get; set; }
        public IReadOnlyCollection<string> Publishers { get; set; }
        public IReadOnlyCollection<EpubCreator> Contributors { get; set; }
        public IReadOnlyCollection<EpubMetadataDate> Dates { get; set; }
        public IReadOnlyCollection<string> Types { get; set; }
        public IReadOnlyCollection<string> Formats { get; set; }
        public IReadOnlyCollection<EpubMetadataIdentifier> Identifiers { get; set; }
        public IReadOnlyCollection<string> Sources { get; set; }
        public IReadOnlyCollection<string> Languages { get; set; }
        public IReadOnlyCollection<string> Relations { get; set; }
        public IReadOnlyCollection<string> Coverages { get; set; }
        public IReadOnlyCollection<string> Rights { get; set; }
        public IReadOnlyCollection<EpubMetadataMeta> MetaItems { get; set; }
    }

    public class EpubCreator
    {
        public string Text { get; internal set; }
        public string Role { get; internal set; }
        public string FileAs { get; internal set; }
        public string AlternateScript { get; internal set; }
    }
}
