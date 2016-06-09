using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using EpubSharp.Entities;
using EpubSharp.Schema.Opf;
using EpubSharp.Utils;

namespace EpubSharp.Readers
{
    internal static class ContentReader
    {
        public static EpubContent ReadContentFiles(ZipArchive epubArchive, EpubBook book)
        {
            var result = new EpubContent
            {
                Html = new Dictionary<string, EpubTextContentFile>(),
                Css = new Dictionary<string, EpubTextContentFile>(),
                Images = new Dictionary<string, EpubByteContentFile>(),
                Fonts = new Dictionary<string, EpubByteContentFile>(),
                AllFiles = new Dictionary<string, EpubContentFile>()
            };
            foreach (var manifestItem in book.Schema.Package.Manifest)
            {
                var contentFilePath = ZipPathUtils.Combine(book.Schema.ContentDirectoryPath, manifestItem.Href);
                var contentFileEntry = epubArchive.GetEntryIgnoringSlashDirection(contentFilePath);
                if (contentFileEntry == null)
                    throw new Exception($"EPUB parsing error: file {contentFilePath} not found in archive.");
                if (contentFileEntry.Length > Int32.MaxValue)
                    throw new Exception($"EPUB parsing error: file {contentFilePath} is bigger than 2 Gb.");
                var fileName = manifestItem.Href;
                var contentMimeType = manifestItem.MediaType;
                var contentType = GetContentTypeByContentMimeType(contentMimeType);
                switch (contentType)
                {
                    case EpubContentType.Xhtml11:
                    case EpubContentType.Css:
                    case EpubContentType.Oeb1Document:
                    case EpubContentType.Oeb1Css:
                    case EpubContentType.Xml:
                    case EpubContentType.Dtbook:
                    case EpubContentType.DtbookNcx:
                        EpubTextContentFile epubTextContentFile = new EpubTextContentFile
                        {
                            FileName = fileName,
                            ContentMimeType = contentMimeType,
                            ContentType = contentType
                        };
                        using (var contentStream = contentFileEntry.Open())
                        {
                            if (contentStream == null)
                                throw new Exception($"Incorrect EPUB file: content file \"{fileName}\" specified in manifest is not found");
                            using (var reader = new StreamReader(contentStream))
                                epubTextContentFile.Content = reader.ReadToEnd();
                        }
                        switch (contentType)
                        {
                            case EpubContentType.Xhtml11:
                                result.Html.Add(fileName, epubTextContentFile);
                                break;
                            case EpubContentType.Css:
                                result.Css.Add(fileName, epubTextContentFile);
                                break;
                        }
                        result.AllFiles.Add(fileName, epubTextContentFile);
                        break;
                    default:
                        var epubByteContentFile = new EpubByteContentFile
                        {
                            FileName = fileName,
                            ContentMimeType = contentMimeType,
                            ContentType = contentType
                        };
                        using (var stream = contentFileEntry.Open())
                        {
                            if (stream == null)
                                throw new Exception($"Incorrect EPUB file: content file \"{fileName}\" specified in manifest is not found");
                            using (var memoryStream = new MemoryStream((int)contentFileEntry.Length))
                            {
                                stream.CopyTo(memoryStream);
                                epubByteContentFile.Content = memoryStream.ToArray();
                            }
                        }
                        switch (contentType)
                        {
                            case EpubContentType.ImageGif:
                            case EpubContentType.ImageJpeg:
                            case EpubContentType.ImagePng:
                            case EpubContentType.ImageSvg:
                                result.Images.Add(fileName, epubByteContentFile);
                                break;
                            case EpubContentType.FontTruetype:
                            case EpubContentType.FontOpentype:
                                result.Fonts.Add(fileName, epubByteContentFile);
                                break;
                        }
                        result.AllFiles.Add(fileName, epubByteContentFile);
                        break;
                }
            }
            return result;
        }

        private static EpubContentType GetContentTypeByContentMimeType(string contentMimeType)
        {
            switch (contentMimeType.ToLowerInvariant())
            {
                case "application/xhtml+xml":
                    return EpubContentType.Xhtml11;
                case "application/x-dtbook+xml":
                    return EpubContentType.Dtbook;
                case "application/x-dtbncx+xml":
                    return EpubContentType.DtbookNcx;
                case "text/x-oeb1-document":
                    return EpubContentType.Oeb1Document;
                case "application/xml":
                    return EpubContentType.Xml;
                case "text/css":
                    return EpubContentType.Css;
                case "text/x-oeb1-css":
                    return EpubContentType.Oeb1Css;
                case "image/gif":
                    return EpubContentType.ImageGif;
                case "image/jpeg":
                    return EpubContentType.ImageJpeg;
                case "image/png":
                    return EpubContentType.ImagePng;
                case "image/svg+xml":
                    return EpubContentType.ImageSvg;
                case "font/truetype":
                    return EpubContentType.FontTruetype;
                case "font/opentype":
                    return EpubContentType.FontOpentype;
                case "application/vnd.ms-opentype":
                    return EpubContentType.FontOpentype;
                default:
                    return EpubContentType.Other;
            }
        }
    }
}
