using System;
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
        private readonly string opfPath = "EPUB/package.opf";
        private readonly string ncxPath = "EPUB/toc.ncx";

        private readonly OpfDocument opf = new OpfDocument
        {
            EpubVersion = EpubVersion.Epub3
        };

        private readonly NcxDocument ncx = new NcxDocument();

        private readonly EpubResources resources = new EpubResources();

        public EpubWriter()
        {
            opf.Metadata.Dates.Add(new OpfMetadataDate { Text = DateTimeOffset.UtcNow.ToString() });
        }

        public EpubWriter(EpubBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (book.Format?.Opf == null) throw new ArgumentException("book opf instance == null", nameof(book));

            opf = book.Format.Opf;
            ncx = book.Format.Ncx;

            resources = book.Resources;

            opfPath = book.Format.Ocf.RootFilePath;
            ncxPath = book.Format.Opf.FindNcxPath();
            if (ncxPath != null)
            {
                ncxPath = PathExt.Combine(PathExt.GetDirectoryPath(opfPath), ncxPath);
            }
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

        public void AddAuthor(string author)
        {
            if (author == null) throw new ArgumentNullException(nameof(author));
            opf.Metadata.Creators.Add(new OpfMetadataCreator { Text = author });
        }
        
        public void AddChapter(string title, string html)
        {
            throw new NotImplementedException("Implement me!");
        }

        public void InsertChapter(string title, string html, int index, EpubChapter parent = null)
        {
            throw new NotImplementedException("Implement me!");
        }

        public void SetCover(byte[] image, ImageFormat format)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

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

            opf.Manifest.Items.Add(new OpfManifestItem
            {
                Id = "cover-image",
                Href = filename,
                MediaType = coverResource.MimeType
            });
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
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                archive.CreateEntry("mimetype", MimeTypeWriter.Format());
                archive.CreateEntry(Constants.OcfPath, OcfWriter.Format(opfPath));
                archive.CreateEntry(opfPath, OpfWriter.Format(opf));

                if (ncx != null)
                {
                    archive.CreateEntry(ncxPath, NcxWriter.Format(ncx));
                }

                var allFiles = new[]
                {
                    resources.Html.Select(dict => dict.Value),
                    resources.Css.Select(dict => dict.Value),
                    resources.Images.Select(dict => dict.Value),
                    resources.Fonts.Select(dict => dict.Value),
                    resources.Other.Select(dict => dict.Value)
                }.SelectMany(collection => collection as EpubContentFile[] ?? collection.ToArray());
                var relativePath = PathExt.GetDirectoryPath(opfPath);
                foreach (var file in allFiles)
                {
                    var absolutePath = PathExt.Combine(relativePath, file.FileName);
                    archive.CreateEntry(absolutePath, file.Content);
                }
            }
        }

        // Old code to add toc.ncx
        /*
            if (opf.Spine.Toc != null)
            {
                var ncxPath = opf.FindNcxPath();
                if (ncxPath == null)
                {
                    throw new EpubWriteException("Spine TOC is set, but NCX path is not.");
                }
                manifest.Add(new XElement(OpfElements.Item, new XAttribute(OpfManifestItem.Attributes.Id, "ncx"), new XAttribute(OpfManifestItem.Attributes.MediaType, ContentType.ContentTypeToMimeType[EpubContentType.DtbookNcx]), new XAttribute(OpfManifestItem.Attributes.Href, ncxPath)));
            }
         */
    }
}
