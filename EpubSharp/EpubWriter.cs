using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using EpubSharp.Format;
using EpubSharp.Format.Writers;

namespace EpubSharp
{
    public enum ImageFormat
    {
        Jpeg, Png
    }

    public class EpubWriter
    {
        private const string OpfPath = "EPUB/package.opf";

        private readonly OpfDocument opf = new OpfDocument
        {
            // We could probably switch to v3 once we can format nav.xhtml
            EpubVersion = EpubVersion.Epub2
        };

        private readonly EpubResources resources = new EpubResources();

        public EpubWriter() { }

        public EpubWriter(EpubBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (book.Format?.Opf == null) throw new ArgumentException("book opf instance == null", nameof(book));

            opf = book.Format.Opf;
            resources = book.Resources;
        }

        public static void Write(EpubBook book, string filename)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullException(nameof(filename));

            var writer = new EpubWriter(book);
            writer.Write(filename);
        }

        public static void Write(EpubBook book, Stream stream)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var writer = new EpubWriter(book);
            writer.Write(stream);
        }

        public void SetCover(byte[] image, ImageFormat format)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var coverItem = new OpfManifestItem { Id = "cover-image" };
            var coverResource = new EpubByteContentFile { Content = image };

            string filename;

            switch (format)
            {
                case ImageFormat.Jpeg:
                    filename = "cover.jpg";
                    coverResource.ContentType = EpubContentType.ImageJpeg;
                    break;
                case ImageFormat.Png:
                    filename = "cover.png";
                    coverResource.ContentType = EpubContentType.ImagePng;
                    break;
                default:
                    throw new ArgumentException($"Unknown format: {format}", nameof(format));
            }

            coverResource.FileName = filename;
            coverResource.MimeType = ContentType.ContentTypeToMimeType[coverResource.ContentType];
            resources.Images.Add(coverResource.FileName, coverResource);

            coverItem.Href = filename;
            coverItem.MediaType = coverResource.MimeType;
            opf.Manifest.Items.Add(coverItem);
        }

        public void AddAuthor(string author)
        {
            if (author == null) throw new ArgumentNullException(nameof(author));
            opf.Metadata.Creators.Add(new OpfMetadataCreator { Text = author });
        }

        public void Write(string filename)
        {
            using (var file = File.Create(filename))
            {
                Write(file);
            }
        }

        public void Write(Stream stream)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                archive.CreateEntry("mimetype", MimeTypeWriter.Format());
                archive.CreateEntry(Constants.OcfPath, OcfWriter.Format(OpfPath));
                archive.CreateEntry(OpfPath, OpfWriter.Format(opf));

                var allFiles = new[]
                {
                    resources.Html.Select(dict => dict.Value).Cast<EpubContentFile>(),
                    resources.Css.Select(dict => dict.Value),
                    resources.Images.Select(dict => dict.Value),
                    resources.Fonts.Select(dict => dict.Value)
                }.SelectMany(collection => collection as EpubContentFile[] ?? collection.ToArray());
                var relativePath = PathExt.GetDirectoryPath(OpfPath);
                foreach (var file in allFiles)
                {
                    var absolutePath = PathExt.Combine(relativePath, file.FileName);
                    archive.CreateEntry(absolutePath, file.Content);
                }
            }
        }
    }
}
