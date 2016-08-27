using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using EpubSharp.Format;
using EpubSharp.Format.Readers;

namespace EpubSharp
{
    public static class EpubReader
    {
        private static readonly IReadOnlyDictionary<string, EpubContentType> MimeTypeToContentType = new Dictionary<string, EpubContentType>
        {
            { "application/xhtml+xml", EpubContentType.Xhtml11 },
            { "application/x-dtbook+xml", EpubContentType.Dtbook },
            { "application/x-dtbncx+xml", EpubContentType.DtbookNcx },
            { "text/x-oeb1-document", EpubContentType.Oeb1Document },
            { "application/xml", EpubContentType.Xml },
            { "text/css", EpubContentType.Css },
            { "text/x-oeb1-css", EpubContentType.Oeb1Css },
            { "image/gif", EpubContentType.ImageGif },
            { "image/jpeg", EpubContentType.ImageJpeg },
            { "image/png", EpubContentType.ImagePng },
            { "image/svg+xml", EpubContentType.ImageSvg },
            { "font/truetype", EpubContentType.FontTruetype },
            { "font/opentype", EpubContentType.FontOpentype },
            { "application/vnd.ms-opentype", EpubContentType.FontOpentype }
        };

        public static EpubBook Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Specified epub file not found.", filePath);
            }

            using (var archive = ZipFile.OpenRead(filePath))
            {
                var format = new EpubFormat();
                format.Ocf = OcfReader.Read(archive.LoadXml("META-INF/container.xml"));
                format.Package = PackageReader.Read(archive.LoadXml(format.Ocf.RootFile));

                // TODO: Implement epub 3.0 nav support and load ncx only if nav is not present.
                if (!string.IsNullOrWhiteSpace(format.Package.NcxPath))
                {
                    var absolutePath = PathExt.Combine(PathExt.GetDirectoryPath(format.Ocf.RootFile), format.Package.NcxPath);
                    format.Ncx = NcxReader.Read(archive.LoadXml(absolutePath));
                    format.NewNcx = NcxReader.Read(archive.LoadXDocument(absolutePath));
                }

                var book = new EpubBook { Format = format };
                book.Content = LoadContent(archive, book);
                book.LazyCoverImage = LazyLoadCoverImage(book);
                book.Chapters = LoadChapters(book, archive);
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
                if (!book.Content.Images.TryGetValue(book.Format.Package.CoverPath, out coverImageContentFile))
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
                return LoadChapterFromNcx(book, book.Format.Ncx.NavigationMap, epubArchive);
            }
            
            return new List<EpubChapter>();
        }

        private static List<EpubChapter> LoadChapterFromNcx(EpubBook book, IReadOnlyCollection<NcxNavigationPoint> navigationPoints, ZipArchive epubArchive)
        {
            var result = new List<EpubChapter>();
            foreach (var navigationPoint in navigationPoints)
            {
                var chapter = new EpubChapter { Title = navigationPoint.LabelText };
                var contentSourceAnchorCharIndex = navigationPoint.ContentSrc.IndexOf('#');
                if (contentSourceAnchorCharIndex == -1)
                {
                    chapter.ContentFileName = navigationPoint.ContentSrc;
                }
                else
                {
                    chapter.ContentFileName = navigationPoint.ContentSrc.Substring(0, contentSourceAnchorCharIndex);
                    chapter.Anchor = navigationPoint.ContentSrc.Substring(contentSourceAnchorCharIndex + 1);
                }

                var contentPath = PathExt.Combine(PathExt.GetDirectoryPath(book.Format.Package.NcxPath), chapter.ContentFileName);
                EpubTextContentFile html;
                if (book.Content.Html.TryGetValue(contentPath, out html))
                {
                    chapter.HtmlContent = html.TextContent;
                }
                else if (book.Content.Images.ContainsKey(contentPath))
                {
                    chapter.HtmlContent = "";
                }
                else
                {
                    throw new EpubException($"Incorrect EPUB manifest: item with href = '{contentPath}' is missing");
                }

                chapter.SubChapters = LoadChapterFromNcx(book, navigationPoint.NavigationPoints, epubArchive);
                result.Add(chapter);
            }
            return result;
        }

        public static EpubContent LoadContent(ZipArchive epubArchive, EpubBook book)
        {
            var result = new EpubContent
            {
                Html = new Dictionary<string, EpubTextContentFile>(),
                Css = new Dictionary<string, EpubTextContentFile>(),
                Images = new Dictionary<string, EpubByteContentFile>(),
                Fonts = new Dictionary<string, EpubByteContentFile>(),
                AllFiles = new Dictionary<string, EpubContentFile>()
            };

            foreach (var item in book.Format.Package.Manifest.Items)
            {
                var path = PathExt.Combine(Path.GetDirectoryName(book.Format.Ocf.RootFile), item.Href);
                var entry = epubArchive.GetEntryIgnoringSlashDirection(path);

                if (entry == null)
                {
                    throw new EpubException($"EPUB parsing error: file {path} not found in archive.");
                }
                if (entry.Length > int.MaxValue)
                {
                    throw new EpubException($"EPUB parsing error: file {path} is bigger than 2 Gb.");
                }

                var fileName = item.Href;
                var mimeType = item.MediaType;

                EpubContentType contentType;
                contentType = MimeTypeToContentType.TryGetValue(mimeType, out contentType)
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
            return result;
        }
    }
}
