using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using EpubSharp.Format;
using EpubSharp.Readers;
using EpubSharp.Schema.Navigation;
using EpubSharp.Utils;

namespace EpubSharp
{
    public static class EpubReader
    {
        public static EpubBook OpenBook(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Specified epub file not found.", filePath);
            var book = new EpubBook { FilePath = filePath };
            using (var archive = ZipFile.OpenRead(filePath))
            {
                book.Format = new EpubFormat();
                book.Format.Ocf = OcfReader.Read(archive);

                var rootFileEntry = archive.GetEntryIgnoringSlashDirection(book.Format.Ocf.RootFile);
                if (rootFileEntry == null)
                    throw new Exception("EPUB parsing error: root file not found in archive.");
                XmlDocument containerDocument;
                using (var containerStream = rootFileEntry.Open())
                {
                    containerDocument = XmlUtils.LoadDocument(containerStream);
                }

                book.Format.Package = PackageDocumentReader.Read(containerDocument);

                string tocId = book.Format.Package.Spine.Toc;
                if (String.IsNullOrEmpty(tocId))
                    throw new Exception("EPUB parsing error: TOC ID is empty.");
                var tocManifestItem = book.Format.Package.Manifest.Items.FirstOrDefault(item => string.Compare(item.Id, tocId, StringComparison.OrdinalIgnoreCase) == 0);
                if (tocManifestItem == null)
                    throw new Exception($"EPUB parsing error: TOC item {tocId} not found in EPUB manifest.");

                var tocFileEntryPath = ZipPathUtils.Combine(Path.GetDirectoryName(book.Format.Ocf.RootFile), tocManifestItem.Href);
                var tocFileEntry = archive.GetEntryIgnoringSlashDirection(tocFileEntryPath);
                if (tocFileEntry == null)
                    throw new Exception($"EPUB parsing error: TOC file {tocFileEntryPath} not found in archive.");
                if (tocFileEntry.Length > int.MaxValue)
                    throw new Exception($"EPUB parsing error: TOC file {tocFileEntryPath} is bigger than 2 Gb.");
                using (var containerStream = tocFileEntry.Open())
                    containerDocument = XmlUtils.LoadDocument(containerStream);

                book.Format.Ncx = NcxReader.Read(containerDocument);
                book.Title = book.Format.Package.Metadata.Titles.FirstOrDefault() ?? string.Empty;
                book.AuthorList = book.Format.Package.Metadata.Creators.Select(creator => creator.Text).ToList();
                book.Author = string.Join(", ", book.AuthorList);
                book.Content = ContentReader.ReadContentFiles(archive, book);
                book.CoverImage = LoadCoverImage(book);
                book.Chapters = LoadChapters(book, archive);
            }
            return book;
        }

        private static Image LoadCoverImage(EpubBook book)
        {
            var metaItems = book.Format.Package.Metadata.MetaItems;
            if (metaItems == null || !metaItems.Any())
                return null;
            var coverMetaItem = metaItems.FirstOrDefault(metaItem => string.Compare(metaItem.Name, "cover", StringComparison.OrdinalIgnoreCase) == 0);
            if (coverMetaItem == null)
                return null;
            if (string.IsNullOrEmpty(coverMetaItem.Content))
                throw new Exception("Incorrect EPUB metadata: cover item content is missing");
            var coverManifestItem = book.Format.Package.Manifest.Items.FirstOrDefault(manifestItem => string.Compare(manifestItem.Id, coverMetaItem.Content, StringComparison.OrdinalIgnoreCase) == 0);
            if (coverManifestItem == null)
                throw new Exception($"Incorrect EPUB manifest: item with ID = \"{coverMetaItem.Content}\" is missing");
            EpubByteContentFile coverImageContentFile;
            if (!book.Content.Images.TryGetValue(coverManifestItem.Href, out coverImageContentFile))
                throw new Exception($"Incorrect EPUB manifest: item with href = \"{coverManifestItem.Href}\" is missing");
            using (var coverImageStream = new MemoryStream(coverImageContentFile.Content))
                return Image.FromStream(coverImageStream);
        }

        private static List<EpubChapter> LoadChapters(EpubBook book, ZipArchive epubArchive)
        {
            return LoadChapters(book, book.Format.Ncx.NavMap, epubArchive);
        }

        private static List<EpubChapter> LoadChapters(EpubBook book, IReadOnlyCollection<EpubNcxNavigationPoint> navigationPoints, ZipArchive epubArchive)
        {
            var result = new List<EpubChapter>();
            foreach (var navigationPoint in navigationPoints)
            {
                var chapter = new EpubChapter { Title = navigationPoint.NavigationLabels.First() };
                var contentSourceAnchorCharIndex = navigationPoint.ContentSource.IndexOf('#');
                if (contentSourceAnchorCharIndex == -1)
                    chapter.ContentFileName = navigationPoint.ContentSource;
                else
                {
                    chapter.ContentFileName = navigationPoint.ContentSource.Substring(0, contentSourceAnchorCharIndex);
                    chapter.Anchor = navigationPoint.ContentSource.Substring(contentSourceAnchorCharIndex + 1);
                }
                EpubTextContentFile htmlContentFile;
                if (!book.Content.Html.TryGetValue(chapter.ContentFileName, out htmlContentFile))
                    throw new Exception($"Incorrect EPUB manifest: item with href = \"{chapter.ContentFileName}\" is missing");
                chapter.HtmlContent = htmlContentFile.Content;
                chapter.SubChapters = LoadChapters(book, navigationPoint.ChildNavigationPoints, epubArchive);
                result.Add(chapter);
            }
            return result;
        }
    }
}
