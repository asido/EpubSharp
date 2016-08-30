using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using EpubSharp.Format;
using EpubSharp.Format.Readers;

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

            using (var archive = ZipFile.Open(filePath, ZipArchiveMode.Read, System.Text.Encoding.UTF8))
            {
                var format = new EpubFormat();
                format.Ocf = OcfReader.Read(archive.LoadXml(Constants.OcfPath));
                format.Opf = OpfReader.Read(archive.LoadXml(format.Ocf.RootFile));

                // TODO: Implement epub 3.0 nav support and load ncx only if nav is not present.
                var ncxPath = format.Opf.FindNcxPath();
                if (ncxPath != null)
                {
                    var absolutePath = PathExt.Combine(PathExt.GetDirectoryPath(format.Ocf.RootFile), ncxPath);
                    format.Ncx = NcxReader.Read(archive.LoadXml(absolutePath));
                }

                var book = new EpubBook { Format = format };
                book.Resources = LoadResources(archive, book);
                book.LazyCoverImage = LazyLoadCoverImage(book);
                book.TableOfContents = LoadChapters(book, archive);
                return book;
            }
        }
        
        private static Lazy<Image> LazyLoadCoverImage(EpubBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (book.Format == null) throw new ArgumentNullException(nameof(book.Format));

            return new Lazy<Image>(() =>
            {
                EpubByteContentFile coverImageContentFile;

                var coverPath = book.Format.Opf.FindCoverPath();
                if (coverPath == null)
                {
                    return null;
                }

                if (!book.Resources.Images.TryGetValue(coverPath, out coverImageContentFile))
                {
                    return null;
                }

                using (var coverImageStream = new MemoryStream(coverImageContentFile.Content))
                {
                    return Image.FromStream(coverImageStream);
                }
            });
        }

        private static List<EpubChapter> LoadChapters(EpubBook book, ZipArchive epubArchive)
        {
            if (book.Format.Ncx != null)
            {
                return LoadChaptersFromNcx(book.Format.Ncx.NavigationMap, epubArchive);
            }
            
            return new List<EpubChapter>();
        }

        private static List<EpubChapter> LoadChaptersFromNcx(IEnumerable<NcxNavigationPoint> navigationPoints, ZipArchive epubArchive)
        {
            var result = new List<EpubChapter>();
            foreach (var navigationPoint in navigationPoints)
            {
                var chapter = new EpubChapter { Title = navigationPoint.LabelText };
                var contentSourceAnchorCharIndex = navigationPoint.ContentSrc.IndexOf('#');
                if (contentSourceAnchorCharIndex == -1)
                {
                    chapter.FileName = navigationPoint.ContentSrc;
                }
                else
                {
                    chapter.FileName = navigationPoint.ContentSrc.Substring(0, contentSourceAnchorCharIndex);
                    chapter.Anchor = navigationPoint.ContentSrc.Substring(contentSourceAnchorCharIndex + 1);
                }

                chapter.SubChapters = LoadChaptersFromNcx(navigationPoint.NavigationPoints, epubArchive);
                result.Add(chapter);
            }
            return result;
        }

        public static EpubResources LoadResources(ZipArchive epubArchive, EpubBook book)
        {
            var result = new EpubResources
            {
                Ocf = new EpubTextContentFile
                {
                    FileName = Constants.OcfPath,
                    ContentType = EpubContentType.Xml,
                    MimeType = ContentType.ContentTypeToMimeType[EpubContentType.Xml],
                    Content = epubArchive.LoadBytes(Constants.OcfPath)
                },
                Opf = new EpubTextContentFile
                {
                    FileName = book.Format.Ocf.RootFile,
                    ContentType = EpubContentType.Xml,
                    MimeType = ContentType.ContentTypeToMimeType[EpubContentType.Xml],
                    Content = epubArchive.LoadBytes(book.Format.Ocf.RootFile)
                },
                Html = new Dictionary<string, EpubTextContentFile>(),
                Css = new Dictionary<string, EpubTextContentFile>(),
                Images = new Dictionary<string, EpubByteContentFile>(),
                Fonts = new Dictionary<string, EpubByteContentFile>(),
                AllFiles = new Dictionary<string, EpubContentFile>(),
                HtmlInReadingOrder = new List<EpubTextContentFile>()
            };

            // Saved items for creating reading order from spine.
            var idToHtmlItems = new Dictionary<string, EpubTextContentFile>();

            foreach (var item in book.Format.Opf.Manifest.Items)
            {
                var path = PathExt.Combine(Path.GetDirectoryName(book.Format.Ocf.RootFile), item.Href);
                var entry = epubArchive.GetEntryIgnoringSlashDirection(path);

                if (entry == null)
                {
                    throw new EpubParseException($"file {path} not found in archive.");
                }
                if (entry.Length > int.MaxValue)
                {
                    throw new EpubParseException($"file {path} is bigger than 2 Gb.");
                }

                var fileName = item.Href;
                var mimeType = item.MediaType;

                EpubContentType contentType;
                contentType = ContentType.MimeTypeToContentType.TryGetValue(mimeType, out contentType)
                    ? contentType
                    : EpubContentType.Other;

                switch (contentType)
                {
                    case EpubContentType.Xhtml11:
                    case EpubContentType.Css:
                    case EpubContentType.Oeb1Document:
                    case EpubContentType.Oeb1Css:
                    case EpubContentType.Xml:
                    case EpubContentType.Dtbook:
                    case EpubContentType.DtbookNcx:
                    {
                        var file = new EpubTextContentFile
                        {
                            FileName = fileName,
                            MimeType = mimeType,
                            ContentType = contentType
                        };

                        using (var stream = entry.Open())
                        {
                            file.Content = stream.ReadToEnd();
                        }

                        switch (contentType)
                        {
                            case EpubContentType.Xhtml11:
                                idToHtmlItems.Add(item.Id, file);
                                result.Html.Add(fileName, file);
                                break;
                            case EpubContentType.Css:
                                result.Css.Add(fileName, file);
                                break;
                        }
                        result.AllFiles.Add(fileName, file);
                        break;
                    }
                    default:
                    {
                        var file = new EpubByteContentFile
                        {
                            FileName = fileName,
                            MimeType = mimeType,
                            ContentType = contentType
                        };

                        using (var stream = entry.Open())
                        {
                            if (stream == null)
                            {
                                throw new EpubException($"Incorrect EPUB file: content file \"{fileName}\" specified in manifest is not found");
                            }

                            using (var memoryStream = new MemoryStream((int) entry.Length))
                            {
                                stream.CopyTo(memoryStream);
                                file.Content = memoryStream.ToArray();
                            }
                        }

                        switch (contentType)
                        {
                            case EpubContentType.ImageGif:
                            case EpubContentType.ImageJpeg:
                            case EpubContentType.ImagePng:
                            case EpubContentType.ImageSvg:
                                result.Images.Add(fileName, file);
                                break;
                            case EpubContentType.FontTruetype:
                            case EpubContentType.FontOpentype:
                                result.Fonts.Add(fileName, file);
                                break;
                        }
                        result.AllFiles.Add(fileName, file);
                        break;
                    }
                }
            }

            foreach (var item in book.Format.Opf.Spine.ItemRefs)
            {
                EpubTextContentFile html;
                if (idToHtmlItems.TryGetValue(item.IdRef, out html))
                {
                    result.HtmlInReadingOrder.Add(html);
                }
            }

            return result;
        }
    }
}
