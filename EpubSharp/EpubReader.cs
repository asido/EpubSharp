using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using EpubSharp.Format;
using EpubSharp.Readers;
using EpubSharp.Schema.Navigation;

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
                book.Format.PackageDocument = SchemaReader.ReadSchema(archive);
                book.Title = book.Format.PackageDocument.Metadata.Titles.FirstOrDefault() ?? string.Empty;
                book.AuthorList = book.Format.PackageDocument.Metadata.Creators.Select(creator => creator.Creator).ToList();
                book.Author = string.Join(", ", book.AuthorList);
                book.Content = ContentReader.ReadContentFiles(archive, book);
                book.CoverImage = LoadCoverImage(book);
                book.Chapters = LoadChapters(book, archive);
            }
            return book;
        }

        private static Image LoadCoverImage(EpubBook book)
        {
            var metaItems = book.Format.PackageDocument.Metadata.MetaItems;
            if (metaItems == null || !metaItems.Any())
                return null;
            var coverMetaItem = metaItems.FirstOrDefault(metaItem => string.Compare(metaItem.Name, "cover", StringComparison.OrdinalIgnoreCase) == 0);
            if (coverMetaItem == null)
                return null;
            if (string.IsNullOrEmpty(coverMetaItem.Content))
                throw new Exception("Incorrect EPUB metadata: cover item content is missing");
            var coverManifestItem = book.Format.PackageDocument.Manifest.FirstOrDefault(manifestItem => string.Compare(manifestItem.Id, coverMetaItem.Content, StringComparison.OrdinalIgnoreCase) == 0);
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
            return LoadChapters(book, book.Format.PackageDocument.Ncx.NavMap, epubArchive);
        }

        private static List<EpubChapter> LoadChapters(EpubBook book, IReadOnlyCollection<EpubNavigationPoint> navigationPoints, ZipArchive epubArchive)
        {
            var result = new List<EpubChapter>();
            foreach (var navigationPoint in navigationPoints)
            {
                var chapter = new EpubChapter { Title = navigationPoint.NavigationLabels.First().Text };
                var contentSourceAnchorCharIndex = navigationPoint.Content.Source.IndexOf('#');
                if (contentSourceAnchorCharIndex == -1)
                    chapter.ContentFileName = navigationPoint.Content.Source;
                else
                {
                    chapter.ContentFileName = navigationPoint.Content.Source.Substring(0, contentSourceAnchorCharIndex);
                    chapter.Anchor = navigationPoint.Content.Source.Substring(contentSourceAnchorCharIndex + 1);
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
