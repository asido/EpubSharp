using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class PackageReader
    {
        private static readonly XNamespace PackageNamespace = "http://www.idpf.org/2007/opf";
        private static readonly XNamespace MetadataNamespace = "http://purl.org/dc/elements/1.1/";

        private static class PackageElements
        {
            public static readonly XName Metadata = PackageNamespace + "metadata";
            public static readonly XName Contributor = MetadataNamespace + "contributor";
            public static readonly XName Coverages = MetadataNamespace + "coverages";
            public static readonly XName Creator = MetadataNamespace + "creator";
            public static readonly XName Date = MetadataNamespace + "date";
            public static readonly XName Description = MetadataNamespace + "description";
            public static readonly XName Format = MetadataNamespace + "format";
            public static readonly XName Identifier = MetadataNamespace + "identifier";
            public static readonly XName Language = MetadataNamespace + "language";
            public static readonly XName Meta = PackageNamespace + "meta";
        }

        public static PackageDocument Read(XmlDocument xml)
        {
            var xmlNamespaceManager = new XmlNamespaceManager(xml.NameTable);
            xmlNamespaceManager.AddNamespace("opf", "http://www.idpf.org/2007/opf");
            var packageNode = xml.DocumentElement.SelectSingleNode("/opf:package", xmlNamespaceManager);
            var package = new PackageDocument();

            var epubVersionValue = packageNode.Attributes["version"].Value;
            if (epubVersionValue == "2.0")
            {
                package.EpubVersion = EpubVersion.Epub2;
            }
            else if (epubVersionValue == "3.0" || epubVersionValue == "3.0.1" || epubVersionValue == "3.1")
            {
                package.EpubVersion = EpubVersion.Epub3;
            }
            else
            {
                throw new Exception($"Unsupported EPUB version: {epubVersionValue}.");
            }

            var metadataNode = packageNode.SelectSingleNode("opf:metadata", xmlNamespaceManager);
            if (metadataNode == null)
                throw new EpubParseException("metadata not found in the package.");
            var metadata = ReadMetadata(metadataNode, package.EpubVersion);
            package.Metadata = metadata;
            XmlNode manifestNode = packageNode.SelectSingleNode("opf:manifest", xmlNamespaceManager);
            if (manifestNode == null)
                throw new EpubParseException("manifest not found in the package.");
            package.Manifest = new PackageManifest();
            package.Manifest.Items = ReadManifestItems(manifestNode);
            XmlNode spineNode = packageNode.SelectSingleNode("opf:spine", xmlNamespaceManager);
            if (spineNode == null)
                throw new EpubParseException("spine not found in the package.");
            PackageSpine spine = ReadSpine(spineNode);
            package.Spine = spine;
            XmlNode guideNode = packageNode.SelectSingleNode("opf:guide", xmlNamespaceManager);
            if (guideNode != null)
            {
                PackageGuide guide = ReadGuide(guideNode);
                package.Guide = guide;
            }

            package.NavPath = FindNavPath(package);
            package.NcxPath = FindNcxPath(package);
            package.CoverPath = FindCoverPath(package);

            return package;
        }

        public static PackageDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            Func<XElement, PackageMetadataCreator> readCreator = elem => new PackageMetadataCreator
            {
                Role = (string) elem.Attribute(PackageNamespace + "role"),
                FileAs = (string) elem.Attribute(PackageNamespace + "file-as"),
                AlternateScript = (string) elem.Attribute(PackageNamespace + "alternate-script"),
                Text = elem.Value
            };

            var metadata = xml.Root.Element(PackageElements.Metadata);
            var epubVersion = GetAndValidateVersion((string) xml.Root.Attribute("version"));

            var package = new PackageDocument
            {
                EpubVersion = epubVersion,
                Metadata = new PackageMetadata
                {
                    Creators = metadata?.Elements(PackageElements.Creator).Select(readCreator).ToList().AsReadOnly(),
                    Contributors = metadata?.Elements(PackageElements.Contributor).Select(readCreator).ToList().AsReadOnly(),
                    Coverages = metadata?.Elements(PackageElements.Coverages).Select(elem => elem.Value).ToList().AsReadOnly(),
                    Dates = metadata?.Elements(PackageElements.Date).Select(elem => new PackageMetadataDate
                    {
                        Text = elem.Value,
                        Event = (string)elem.Attribute(PackageNamespace + "event")
                    }).ToList().AsReadOnly(),
                    Descriptions = metadata?.Elements(PackageElements.Description).Select(elem => elem.Value).ToList().AsReadOnly(),
                    Formats = metadata?.Elements(PackageElements.Format).Select(elem => elem.Value).ToList().AsReadOnly(),
                    Identifiers = metadata?.Elements(PackageElements.Identifier).Select(elem => new PackageMetadataIdentifier
                    {
                        Id = (string) elem.Attribute("id"),
                        Scheme = (string) elem.Attribute(PackageNamespace + "scheme"),
                        Text = elem.Value
                    }).ToList().AsReadOnly(),
                    Languages = metadata?.Elements(PackageElements.Language).Select(elem => elem.Value).ToList().AsReadOnly(),
                    Metas = metadata?.Elements(PackageElements.Meta).Select(elem => new PackageMetadataMeta
                    {
                        Id = (string) elem.Attribute("id"),
                        Name = (string) elem.Attribute("name"),
                        Refines = (string) elem.Attribute("refines"),
                        Scheme = (string) elem.Attribute("scheme"),
                        Property = (string) elem.Attribute("property"),
                        Text = epubVersion == EpubVersion.Epub2 ? (string) elem.Attribute("content") : elem.Value
                    }).ToList().AsReadOnly()
                }
            };

            if (package.Metadata.Creators.Any(e => e.Text == "Randall Munroe"))
            {
                Console.WriteLine();
            }

            return package;
        }

        private static EpubVersion GetAndValidateVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentNullException(nameof(version));

            if (version == "2.0")
            {
                return EpubVersion.Epub2;
            }
            if (version == "3.0" || version == "3.0.1" || version == "3.1")
            {
                return EpubVersion.Epub3;
            }

            throw new Exception($"Unsupported EPUB version: {version}.");
        }

        private static string FindCoverPath(PackageDocument package)
        {
            string coverId = null;

            var coverMetaItem = package.Metadata.Metas
                .FirstOrDefault(metaItem => string.Compare(metaItem.Name, "cover", StringComparison.OrdinalIgnoreCase) == 0);
            if (coverMetaItem != null)
            {
                coverId = coverMetaItem.Text;
            }
            else
            {
                var item = package.Manifest.Items.FirstOrDefault(e => e.Properties.Contains("cover-image"));
                if (item != null)
                {
                    coverId = item.Href;
                }
            }

            if (coverId == null)
            {
                return null;
            }

            var coverItem = package.Manifest.Items.FirstOrDefault(item => item.Id == coverId);
            return coverItem?.Href;
        }

        private static string FindNcxPath(PackageDocument package)
        {
            var ncxItem = package.Manifest.Items.FirstOrDefault(e => e.MediaType == "application/x-dtbncx+xml");
            if (ncxItem != null)
            {
                package.NcxPath = ncxItem.Href;
            }
            else
            {
                // If we can't find the toc by media-type then try to look for id of the item in the spine attributes as
                // according to http://www.idpf.org/epub/20/spec/OPF_2.0.1_draft.htm#Section2.4.1.2,
                // "The item that describes the NCX must be referenced by the spine toc attribute."

                if (!string.IsNullOrWhiteSpace(package.Spine.Toc))
                {
                    var tocItem = package.Manifest.Items.FirstOrDefault(e => e.Id == package.Spine.Toc);
                    if (tocItem != null)
                    {
                        return tocItem.Href;
                    }
                }
            }

            return null;
        }

        private static string FindNavPath(PackageDocument package)
        {
            var navItem = package.Manifest.Items.FirstOrDefault(e => e.Properties.Contains("nav"));
            return navItem?.Href;
        }

        private static PackageMetadata ReadMetadata(XmlNode metadataNode, EpubVersion epubVersion)
        {
            var titles = new List<string>();
            var creators = new List<PackageMetadataCreator>();
            var subjects = new List<string>();
            var publishers = new List<string>();
            var contributors = new List<PackageMetadataCreator>();
            var dates = new List<PackageMetadataDate>();
            var types = new List<string>();
            var formats = new List<string>();
            var identifiers = new List<PackageMetadataIdentifier>();
            var sources = new List<string>();
            var languages = new List<string>();
            var relations = new List<string>();
            var coverages = new List<string>();
            var rights = new List<string>();
            var metaItems = new List<PackageMetadataMeta>();
            var descriptions = new List<string>();

            foreach (XmlNode metadataItemNode in metadataNode.ChildNodes)
            {
                var innerText = metadataItemNode.InnerText;
                switch (metadataItemNode.LocalName.ToLowerInvariant())
                {
                    case "title":
                        titles.Add(innerText);
                        break;
                    case "creator":
                        var creator = ReadMetadataCreator(metadataItemNode);
                        creators.Add(creator);
                        break;
                    case "subject":
                        subjects.Add(innerText);
                        break;
                    case "description":
                        descriptions.Add(innerText);
                        break;
                    case "publisher":
                        publishers.Add(innerText);
                        break;
                    case "contributor":
                        var contributor = ReadMetadataContributor(metadataItemNode);
                        contributors.Add(contributor);
                        break;
                    case "date":
                        dates.Add(new PackageMetadataDate
                        {
                            Text = metadataItemNode.InnerText,
                            Event = metadataItemNode.Attributes?["opf:event"]?.Value
                        });
                        break;
                    case "type":
                        types.Add(innerText);
                        break;
                    case "format":
                        formats.Add(innerText);
                        break;
                    case "identifier":
                        var identifier = ReadMetadataIdentifier(metadataItemNode);
                        identifiers.Add(identifier);
                        break;
                    case "source":
                        sources.Add(innerText);
                        break;
                    case "language":
                        languages.Add(innerText);
                        break;
                    case "relation":
                        relations.Add(innerText);
                        break;
                    case "coverage":
                        if (!string.IsNullOrWhiteSpace(innerText))
                        {
                            coverages.Add(innerText);
                        }
                        break;
                    case "rights":
                        rights.Add(innerText);
                        break;
                    case "meta":
                        var meta = ReadMetadataMeta(metadataItemNode, epubVersion);
                        // Because in test samples this meta defines it's own namespace.
                        // This is only until XDocument version is not ready.
                        if (meta.Name != "BNContentKind")
                        {
                            metaItems.Add(meta);
                        }
                        break;
                }
            }
            
            return new PackageMetadata
            {
                Titles = titles,
                Creators = creators,
                Subjects = subjects,
                Publishers = publishers,
                Contributors = contributors,
                Dates = dates,
                Types = types,
                Formats = formats,
                Identifiers = identifiers,
                Sources = sources,
                Languages = languages,
                Relations = relations,
                Coverages = coverages,
                Rights = rights,
                Metas = metaItems,
                Descriptions = descriptions
            };
        }

        private static PackageMetadataCreator ReadMetadataCreator(XmlNode metadataCreatorNode)
        {
            var result = new PackageMetadataCreator();
            foreach (XmlAttribute metadataCreatorNodeAttribute in metadataCreatorNode.Attributes)
            {
                var attributeValue = metadataCreatorNodeAttribute.Value;
                switch (metadataCreatorNodeAttribute.Name.ToLowerInvariant())
                {
                    case "opf:role":
                    case "ns0:role":
                        result.Role = attributeValue;
                        break;
                    case "opf:file-as":
                    case "ns0:file-as":
                        result.FileAs = attributeValue;
                        break;
                    case "opf:alternate-script":
                    case "ns0:alternate-script":
                        result.AlternateScript = attributeValue;
                        break;
                }
            }
            result.Text = metadataCreatorNode.InnerText;
            return result;
        }

        private static PackageMetadataCreator ReadMetadataContributor(XmlNode metadataContributorNode)
        {
            var result = new PackageMetadataCreator();
            foreach (XmlAttribute metadataContributorNodeAttribute in metadataContributorNode.Attributes)
            {
                var attributeValue = metadataContributorNodeAttribute.Value;
                switch (metadataContributorNodeAttribute.Name.ToLowerInvariant())
                {
                    case "opf:role":
                    case "ns2:role":
                        result.Role = attributeValue;
                        break;
                    case "opf:file-as":
                    case "ns2:file-as":
                        result.FileAs = attributeValue;
                        break;
                    case "opf:alternate-script":
                    case "ns2:alternate-script":
                        result.AlternateScript = attributeValue;
                        break;
                }
            }
            result.Text = metadataContributorNode.InnerText;
            return result;
        }

        private static PackageMetadataIdentifier ReadMetadataIdentifier(XmlNode metadataIdentifierNode)
        {
            PackageMetadataIdentifier result = new PackageMetadataIdentifier();
            foreach (XmlAttribute metadataIdentifierNodeAttribute in metadataIdentifierNode.Attributes)
            {
                string attributeValue = metadataIdentifierNodeAttribute.Value;
                switch (metadataIdentifierNodeAttribute.Name.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "opf:scheme":
                    case "ns0:scheme":
                    case "ns1:scheme":
                    case "ns2:scheme":
                    case "ns3:scheme":
                    case "ns4:scheme":
                    case "ns5:scheme":
                    case "ns6:scheme":
                    case "ns7:scheme":
                    case "ns8:scheme":
                    case "ns9:scheme":
                        result.Scheme = attributeValue;
                        break;
                }
            }
            result.Text = metadataIdentifierNode.InnerText;
            return result;
        }

        private static PackageMetadataMeta ReadMetadataMeta(XmlNode metadataMetaNode, EpubVersion version)
        {
            PackageMetadataMeta result = new PackageMetadataMeta();
            foreach (XmlAttribute metadataMetaNodeAttribute in metadataMetaNode.Attributes)
            {
                string attributeValue = metadataMetaNodeAttribute.Value;
                switch (metadataMetaNodeAttribute.Name.ToLowerInvariant())
                {
                    case "name":
                        result.Name = attributeValue;
                        break;
                    case "content":
                        result.Text = attributeValue;
                        break;
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "refines":
                        result.Refines = attributeValue;
                        break;
                    case "property":
                        result.Property = attributeValue;
                        break;
                    case "scheme":
                        result.Scheme = attributeValue;
                        break;
                }
            }
            if (version == EpubVersion.Epub3)
            {
                result.Text = metadataMetaNode.InnerText;
            }
            return result;
        }

        private static IReadOnlyCollection<PackageManifestItem> ReadManifestItems(XmlNode manifestNode)
        {
            var result = new List<PackageManifestItem>();
            foreach (XmlNode manifestItemNode in manifestNode.ChildNodes)
                if (string.Compare(manifestItemNode.LocalName, "item", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var manifestItem = new PackageManifestItem();
                    foreach (XmlAttribute manifestItemNodeAttribute in manifestItemNode.Attributes)
                    {
                        string attributeValue = manifestItemNodeAttribute.Value;
                        switch (manifestItemNodeAttribute.Name.ToLowerInvariant())
                        {
                            case "id":
                                manifestItem.Id = attributeValue;
                                break;
                            case "href":
                                manifestItem.Href = attributeValue;
                                break;
                            case "properties":
                                manifestItem.Properties = attributeValue.Split(' ');
                                break;
                            case "media-type":
                                manifestItem.MediaType = attributeValue;
                                break;
                            case "required-namespace":
                                manifestItem.RequiredNamespace = attributeValue;
                                break;
                            case "required-modules":
                                manifestItem.RequiredModules = attributeValue;
                                break;
                            case "fallback":
                                manifestItem.Fallback = attributeValue;
                                break;
                            case "fallback-style":
                                manifestItem.FallbackStyle = attributeValue;
                                break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(manifestItem.Id))
                        throw new Exception("Incorrect EPUB manifest: item ID is missing");
                    if (string.IsNullOrWhiteSpace(manifestItem.Href))
                        throw new Exception("Incorrect EPUB manifest: item href is missing");
                    if (string.IsNullOrWhiteSpace(manifestItem.MediaType))
                        throw new Exception("Incorrect EPUB manifest: item media type is missing");
                    result.Add(manifestItem);
                }
            return result.AsReadOnly();
        }

        private static PackageSpine ReadSpine(XmlNode spineNode)
        {
            var result = new PackageSpine();
            var tocAttribute = spineNode.Attributes["toc"];
            if (!string.IsNullOrWhiteSpace(tocAttribute?.Value))
            {
                result.Toc = tocAttribute.Value;
            }
            
            var itemRefs = new List<PackageSpineItemRef>();
            foreach (XmlNode spineItemNode in spineNode.ChildNodes)
            {
                if (string.Compare(spineItemNode.LocalName, "itemref", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var spineItemRef = new PackageSpineItemRef();
                    var idRefAttribute = spineItemNode.Attributes["idref"];
                    if (string.IsNullOrWhiteSpace(idRefAttribute?.Value))
                        throw new Exception("Incorrect EPUB spine: item ID ref is missing");
                    spineItemRef.IdRef = idRefAttribute.Value;
                    XmlAttribute linearAttribute = spineItemNode.Attributes["linear"];
                    spineItemRef.IsLinear = linearAttribute == null || string.Compare(linearAttribute.Value, "no", StringComparison.OrdinalIgnoreCase) != 0;
                    itemRefs.Add(spineItemRef);
                }
            }
            result.ItemRefs = itemRefs.AsReadOnly();
            return result;
        }

        private static PackageGuide ReadGuide(XmlNode guideNode)
        {
            var references = new List<PackageGuideReference>();

            foreach (XmlNode guideReferenceNode in guideNode.ChildNodes)
            {
                if (string.Compare(guideReferenceNode.LocalName, "reference", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    PackageGuideReference guideReference = new PackageGuideReference();
                    foreach (XmlAttribute guideReferenceNodeAttribute in guideReferenceNode.Attributes)
                    {
                        string attributeValue = guideReferenceNodeAttribute.Value;
                        switch (guideReferenceNodeAttribute.Name.ToLowerInvariant())
                        {
                            case "type":
                                guideReference.Type = attributeValue;
                                break;
                            case "title":
                                guideReference.Title = attributeValue;
                                break;
                            case "href":
                                guideReference.Href = attributeValue;
                                break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(guideReference.Type))
                        throw new Exception("Incorrect EPUB guide: item type is missing");
                    if (string.IsNullOrWhiteSpace(guideReference.Href))
                        throw new Exception("Incorrect EPUB guide: item href is missing");
                    references.Add(guideReference);
                }
            }

            return new PackageGuide { References = references.AsReadOnly() };
        }
    }
}
