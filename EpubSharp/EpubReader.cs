using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using EpubSharp.Format;
using EpubSharp.Format.Readers;

namespace EpubSharp
{
    public static class EpubReader
    {
        public static EpubBook Read(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Specified epub file not found.", filePath);
            }

            return Read(File.Open(filePath, FileMode.Open, FileAccess.Read), false);
        }

        public static EpubBook Read(Stream stream, bool leaveOpen)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen, Encoding.UTF8))
            {
                var format = new EpubFormat { Ocf = OcfReader.Read(archive.LoadXml(Constants.OcfPath)) };

                var rootFilePath = format.Ocf.RootFilePath;
                if (rootFilePath == null)
                {
                    throw new EpubParseException("Epub OCF doesn't specify a root file.");
                }

                format.Opf = OpfReader.Read(archive.LoadXml(rootFilePath));

                // TODO: Implement epub 3.0 nav support and load ncx only if nav is not present.
                var ncxPath = format.Opf.FindNcxPath();
                if (ncxPath != null)
                {
                    var absolutePath = PathExt.Combine(PathExt.GetDirectoryPath(rootFilePath), ncxPath);
                    format.Ncx = NcxReader.Read(archive.LoadXml(absolutePath));
                }

                var book = new EpubBook { Format = format };
                book.Resources = LoadResources(archive, book);
                book.SpecialResources = LoadSpecialResources(archive, book);
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
                return LoadChaptersFromNcx(book.Format.Ncx.NavMap.NavPoints, epubArchive);
            }
            
            return new List<EpubChapter>();
        }

        private static List<EpubChapter> LoadChaptersFromNcx(IEnumerable<NcxNavPoint> navigationPoints, ZipArchive epubArchive)
        {
            var result = new List<EpubChapter>();
            foreach (var navigationPoint in navigationPoints)
            {
                var chapter = new EpubChapter { Title = navigationPoint.NavLabelText };
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

                chapter.SubChapters = LoadChaptersFromNcx(navigationPoint.NavPoints, epubArchive);
                result.Add(chapter);
            }
            return result;
        }

        public static EpubResources LoadResources(ZipArchive epubArchive, EpubBook book)
        {
            var result = new EpubResources
            {
                Html = new Dictionary<string, EpubTextContentFile>(),
                Css = new Dictionary<string, EpubTextContentFile>(),
                Images = new Dictionary<string, EpubByteContentFile>(),
                Fonts = new Dictionary<string, EpubByteContentFile>()
            };

            foreach (var item in book.Format.Opf.Manifest.Items)
            {
                var path = PathExt.Combine(Path.GetDirectoryName(book.Format.Ocf.RootFilePath), item.Href);
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
                                result.Html.Add(fileName, file);
                                break;
                            case EpubContentType.Css:
                                result.Css.Add(fileName, file);
                                break;
                            default:
                                result.Other.Add(fileName, file);
                                break;
                            }
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
                            default:
                                result.Other.Add(fileName, file);
                                break;
                        }
                        break;
                    }
                }
            }
            
            return result;
        }

        private static EpubSpecialResources LoadSpecialResources(ZipArchive epubArchive, EpubBook book)
        {
            var result = new EpubSpecialResources
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
                    FileName = book.Format.Ocf.RootFilePath,
                    ContentType = EpubContentType.Xml,
                    MimeType = ContentType.ContentTypeToMimeType[EpubContentType.Xml],
                    Content = epubArchive.LoadBytes(book.Format.Ocf.RootFilePath)
                },
                HtmlInReadingOrder = new List<EpubTextContentFile>()
            };

            var htmlFiles = book.Format.Opf.Manifest.Items
                .Where(item => ContentType.MimeTypeToContentType.ContainsKey(item.MediaType) && ContentType.MimeTypeToContentType[item.MediaType] == EpubContentType.Xhtml11)
                .ToDictionary(item => item.Id, item => item.Href);

            foreach (var item in book.Format.Opf.Spine.ItemRefs)
            {
                string href;
                if (!htmlFiles.TryGetValue(item.IdRef, out href))
                {
                    continue;
                }

                EpubTextContentFile html;
                if (book.Resources.Html.TryGetValue(href, out html))
                {
                    result.HtmlInReadingOrder.Add(html);
                }
            }

            return result;
        }
    }
}
