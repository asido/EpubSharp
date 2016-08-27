using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EpubSharp.Format;
using EpubSharp.Format.Readers;
using EpubSharp.Readers;
using EpubSharp.Utils;

namespace EpubSharp
{
    public static class EpubReader
    {
        public static EpubBook Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Specified epub file not found.", filePath);
            }

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

                book.Format.Package = PackageReader.Read(containerDocument);

                LoadNcx(archive, book);

                book.Title = book.Format.Package.Metadata.Titles.FirstOrDefault() ?? string.Empty;
                book.AuthorList = book.Format.Package.Metadata.Creators.Select(creator => creator.Text).ToList();
                book.Author = string.Join(", ", book.AuthorList);
                book.Content = ContentReader.ReadContentFiles(archive, book);
                book.CoverImage = LoadCoverImage(book);
                book.Chapters = LoadChapters(book, archive);
            }
            return book;
        }

        private static void LoadNcx(ZipArchive archive, EpubBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));

            var tocId = book.Format.Package.Spine.Toc;
            if (string.IsNullOrEmpty(tocId))
            {
                return;
            }

            var tocManifestItem = book.Format.Package.Manifest.Items.FirstOrDefault(item => string.Compare(item.Id, tocId, StringComparison.OrdinalIgnoreCase) == 0);
            if (tocManifestItem == null)
            {
                //throw new Exception($"EPUB parsing error: TOC item {tocId} not found in EPUB manifest.");
                return;
            }

            var tocFileEntryPath = ZipPathUtils.Combine(Path.GetDirectoryName(book.Format.Ocf.RootFile), tocManifestItem.Href);
            var tocFileEntry = archive.GetEntryIgnoringSlashDirection(tocFileEntryPath);
            if (tocFileEntry == null)
            {
                //throw new Exception($"EPUB parsing error: TOC file {tocFileEntryPath} not found in archive.");
                return;
            }
            if (tocFileEntry.Length > int.MaxValue)
            {
                //throw new Exception($"EPUB parsing error: TOC file {tocFileEntryPath} is bigger than 2 Gb.");
                return;
            }

            using (var containerStream = tocFileEntry.Open())
            {
                var doc = XmlUtils.LoadDocument(containerStream);
                book.Format.Ncx = NcxReader.Read(doc);
            }
            book.Format.NewNcx = NcxReader.Read(XDocument.Load(tocFileEntry.Open()));
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
            if (book.Format.Ncx != null)
            {
                return LoadChapterFromNcx(book, book.Format.Ncx.NavigationMap, epubArchive);
            }
            
            return new List<EpubChapter>();
        }

        private static List<EpubChapter> LoadChapterFromNcx(EpubBook book, IReadOnlyCollection<EpubNcxNavigationPoint> navigationPoints, ZipArchive epubArchive)
        {
            var result = new List<EpubChapter>();
            foreach (var navigationPoint in navigationPoints)
            {
                var chapter = new EpubChapter { Title = navigationPoint.Label };
                var contentSourceAnchorCharIndex = navigationPoint.ContentSource.IndexOf('#');
                if (contentSourceAnchorCharIndex == -1)
                    chapter.ContentFileName = navigationPoint.ContentSource;
                else
                {
                    chapter.ContentFileName = navigationPoint.ContentSource.Substring(0, contentSourceAnchorCharIndex);
                    chapter.Anchor = navigationPoint.ContentSource.Substring(contentSourceAnchorCharIndex + 1);
                }

                var contentPath = ZipPathUtils.Combine(ZipPathUtils.GetDirectoryPath(book.Format.Package.NcxPath), chapter.ContentFileName);
                EpubTextContentFile html;
                if (book.Content.Html.TryGetValue(contentPath, out html))
                {
                    chapter.HtmlContent = html.Content;
                }
                else if (book.Content.Images.ContainsKey(contentPath))
                {
                    chapter.HtmlContent = "";
                }
                else
                {
                    throw new Exception($"Incorrect EPUB manifest: item with href = '{contentPath}' is missing");
                }

                chapter.SubChapters = LoadChapterFromNcx(book, navigationPoint.NavigationPoints, epubArchive);
                result.Add(chapter);
            }
            return result;
        }
    }
}
