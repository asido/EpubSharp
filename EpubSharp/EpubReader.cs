using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EpubSharp.Format;
using EpubSharp.Format.Readers;

namespace EpubSharp
{
    public static class EpubReader
    {
        public static EpubBook Read(string filePath, Encoding encoding = null)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (encoding == null) encoding = Constants.DefaultEncoding;

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Specified epub file not found.", filePath);
            }

            return Read(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), false, encoding);
        }

        public static EpubBook Read(byte[] epubData, Encoding encoding = null)
        {
            if (encoding == null) encoding = Constants.DefaultEncoding;
            return Read(new MemoryStream(epubData), false, encoding);
        }

        public static EpubBook Read(Stream stream, bool leaveOpen, Encoding encoding = null)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) encoding = Constants.DefaultEncoding;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen, encoding))
            {
                var format = new EpubFormat { Ocf = OcfReader.Read(archive.LoadXml(Constants.OcfPath)) };

                format.Paths.OcfAbsolutePath = Constants.OcfPath;

                format.Paths.OpfAbsolutePath = format.Ocf.RootFilePath;
                if (format.Paths.OpfAbsolutePath == null)
                {
                    throw new EpubParseException("Epub OCF doesn't specify a root file.");
                }

                format.Opf = OpfReader.Read(archive.LoadXml(format.Paths.OpfAbsolutePath));

                var navPath = format.Opf.FindNavPath();
                if (navPath != null)
                {
                    format.Paths.NavAbsolutePath = navPath.ToAbsolutePath(format.Paths.OpfAbsolutePath);
                    format.Nav = NavReader.Read(archive.LoadHtml(format.Paths.NavAbsolutePath));
                }

                var ncxPath = format.Opf.FindNcxPath();
                if (ncxPath != null)
                {
                    format.Paths.NcxAbsolutePath = ncxPath.ToAbsolutePath(format.Paths.OpfAbsolutePath);
                    format.Ncx = NcxReader.Read(archive.LoadXml(format.Paths.NcxAbsolutePath));
                }

                var book = new EpubBook { Format = format };
                book.Resources = LoadResources(archive, book);
                book.SpecialResources = LoadSpecialResources(archive, book);
                book.CoverImage = LoadCoverImage(book);
                book.TableOfContents = LoadChapters(book);
                return book;
            }
        }

        private static byte[] LoadCoverImage(EpubBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (book.Format == null) throw new ArgumentNullException(nameof(book.Format));

            var coverPath = book.Format.Opf.FindCoverPath();
            if (coverPath == null)
            {
                return null;
            }

            var coverImageFile = book.Resources.Images.SingleOrDefault(e => e.Href == coverPath);
            return coverImageFile?.Content;
        }

        private static List<EpubChapter> LoadChapters(EpubBook book)
        {
            if (book.Format.Nav != null)
            {
                var tocNav = book.Format.Nav.Body.Navs.SingleOrDefault(e => e.Type == NavNav.Attributes.TypeValues.Toc);
                if (tocNav != null)
                {
                    return LoadChaptersFromNav(book.Format.Paths.NavAbsolutePath, tocNav.Dom);
                }
            }

            if (book.Format.Ncx != null)
            {
                return LoadChaptersFromNcx(book.Format.Paths.NcxAbsolutePath, book.Format.Ncx.NavMap.NavPoints);
            }

            return new List<EpubChapter>();
        }

        private static List<EpubChapter> LoadChaptersFromNav(string navAbsolutePath, XElement element, EpubChapter parentChapter = null)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            var ns = element.Name.Namespace;

            var result = new List<EpubChapter>();
            var previous = parentChapter;

            var ol = element.Element(ns + NavElements.Ol);
            if (ol == null)
                return result;

            foreach (var li in ol.Elements(ns + NavElements.Li))
            {
                var chapter = new EpubChapter
                {
                    Parent = parentChapter,
                    Previous = previous
                };

                if (previous != null)
                    previous.Next = chapter;

                var link = li.Element(ns + NavElements.A);
                if (link != null)
                {
                    var id = link.Attribute("id")?.Value;
                    if (id != null)
                    {
                        chapter.Id = id;
                    }

                    var url = link.Attribute("href")?.Value;
                    if (url != null)
                    {
                        var href = new Href(url);
                        chapter.RelativePath = href.Path;
                        chapter.HashLocation = href.HashLocation;
                        chapter.AbsolutePath = chapter.RelativePath.ToAbsolutePath(navAbsolutePath);
                    }

                    var titleTextElement = li.Descendants().FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Value));
                    if (titleTextElement != null)
                    {
                        chapter.Title = titleTextElement.Value;
                    }

                    if (li.Element(ns + NavElements.Ol) != null)
                    {
                        chapter.SubChapters = LoadChaptersFromNav(navAbsolutePath, li, chapter);
                    }
                    result.Add(chapter);

                    previous = chapter.SubChapters.Any() ? chapter.SubChapters.Last() : chapter;
                }
            }

            return result;
        }

        private static List<EpubChapter> LoadChaptersFromNcx(string ncxAbsolutePath, IEnumerable<NcxNavPoint> navigationPoints, EpubChapter parentChapter = null)
        {
            var result = new List<EpubChapter>();
            var previous = parentChapter;

            foreach (var navigationPoint in navigationPoints)
            {
                var chapter = new EpubChapter
                {
                    Title = navigationPoint.NavLabelText,
                    Parent = parentChapter,
                    Previous = previous
                };

                if (previous != null)
                    previous.Next = chapter;

                var href = new Href(navigationPoint.ContentSrc);
                chapter.RelativePath = href.Path;
                chapter.AbsolutePath = href.Path.ToAbsolutePath(ncxAbsolutePath);
                chapter.HashLocation = href.HashLocation;
                chapter.SubChapters = LoadChaptersFromNcx(ncxAbsolutePath, navigationPoint.NavPoints, chapter);
                result.Add(chapter);

                previous = chapter.SubChapters.Any() ? chapter.SubChapters.Last() : chapter;
            }
            return result;
        }

        private static EpubResources LoadResources(ZipArchive epubArchive, EpubBook book)
        {
            var resources = new EpubResources();

            foreach (var item in book.Format.Opf.Manifest.Items)
            {
                var path = item.Href.ToAbsolutePath(book.Format.Paths.OpfAbsolutePath);
                var entry = epubArchive.GetEntryImproved(path);

                if (entry == null)
                {
                    throw new EpubParseException($"file {path} not found in archive.");
                }
                if (entry.Length > int.MaxValue)
                {
                    throw new EpubParseException($"file {path} is bigger than 2 Gb.");
                }

                var href = item.Href;
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
                            var file = new EpubTextFile
                            {
                                AbsolutePath = path,
                                Href = href,
                                MimeType = mimeType,
                                ContentType = contentType
                            };

                            resources.All.Add(file);

                            using (var stream = entry.Open())
                            {
                                file.Content = stream.ReadToEnd();
                            }

                            switch (contentType)
                            {
                                case EpubContentType.Xhtml11:
                                    resources.Html.Add(file);
                                    break;
                                case EpubContentType.Css:
                                    resources.Css.Add(file);
                                    break;
                                default:
                                    resources.Other.Add(file);
                                    break;
                            }
                            break;
                        }
                    default:
                        {
                            var file = new EpubByteFile
                            {
                                AbsolutePath = path,
                                Href = href,
                                MimeType = mimeType,
                                ContentType = contentType
                            };

                            resources.All.Add(file);

                            using (var stream = entry.Open())
                            {
                                if (stream == null)
                                {
                                    throw new EpubException($"Incorrect EPUB file: content file \"{href}\" specified in manifest is not found");
                                }

                                using (var memoryStream = new MemoryStream((int)entry.Length))
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
                                    resources.Images.Add(file);
                                    break;
                                case EpubContentType.FontTruetype:
                                case EpubContentType.FontOpentype:
                                    resources.Fonts.Add(file);
                                    break;
                                default:
                                    resources.Other.Add(file);
                                    break;
                            }
                            break;
                        }
                }
            }

            return resources;
        }

        private static EpubSpecialResources LoadSpecialResources(ZipArchive epubArchive, EpubBook book)
        {
            var result = new EpubSpecialResources
            {
                Ocf = new EpubTextFile
                {
                    AbsolutePath = Constants.OcfPath,
                    Href = Constants.OcfPath,
                    ContentType = EpubContentType.Xml,
                    MimeType = ContentType.ContentTypeToMimeType[EpubContentType.Xml],
                    Content = epubArchive.LoadBytes(Constants.OcfPath)
                },
                Opf = new EpubTextFile
                {
                    AbsolutePath = book.Format.Paths.OpfAbsolutePath,
                    Href = book.Format.Paths.OpfAbsolutePath,
                    ContentType = EpubContentType.Xml,
                    MimeType = ContentType.ContentTypeToMimeType[EpubContentType.Xml],
                    Content = epubArchive.LoadBytes(book.Format.Paths.OpfAbsolutePath)
                },
                HtmlInReadingOrder = new List<EpubTextFile>()
            };

            var htmlFiles = book.Format.Opf.Manifest.Items
                .Where(item => ContentType.MimeTypeToContentType.ContainsKey(item.MediaType) && ContentType.MimeTypeToContentType[item.MediaType] == EpubContentType.Xhtml11)
                .ToDictionary(item => item.Id, item => item.Href);

            foreach (var item in book.Format.Opf.Spine.ItemRefs)
            {
                if (!htmlFiles.TryGetValue(item.IdRef, out string href))
                {
                    continue;
                }

                var html = book.Resources.Html.SingleOrDefault(e => e.Href == href);
                if (html != null)
                {
                    result.HtmlInReadingOrder.Add(html);
                }
            }

            return result;
        }
    }
}
