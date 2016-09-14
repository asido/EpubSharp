using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using EpubSharp.Format;
using EpubSharp.Format.Writers;

namespace EpubSharp
{
    public class EpubWriter
    {
        private readonly string opfPath = "EPUB/package.opf";
        private readonly string ncxPath = "EPUB/toc.ncx";

        private readonly OpfDocument opf = new OpfDocument
        {
            EpubVersion = EpubVersion.Epub3
        };

        private readonly NavDocument nav = new NavDocument();
        private readonly NcxDocument ncx = new NcxDocument();

        private readonly EpubResources resources = new EpubResources();

        public EpubWriter()
        {
            opf.Metadata.Dates.Add(new OpfMetadataDate { Text = DateTimeOffset.UtcNow.ToString("o") });
            opf.Manifest.Items.Add(new OpfManifestItem { Id = "ncx", Href = "toc.ncx", MediaType = ContentType.ContentTypeToMimeType[EpubContentType.DtbookNcx] });
            opf.Spine.Toc = "ncx";
        }

        public EpubWriter(EpubBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (book.Format?.Opf == null) throw new ArgumentException("book opf instance == null", nameof(book));

            opf = book.Format.Opf;
            nav = book.Format.Nav;
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

        /// <summary>
        /// Clones the book instance by writing and reading it from memory.
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public static EpubBook MakeCopy(EpubBook book)
        {
            var stream = new MemoryStream();
            var writer = new EpubWriter(book);
            writer.Write(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var epub = EpubReader.Read(stream, false);
            return epub;
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

        public void RemoveCover()
        {
            var path = opf.FindAndRemoveCover();
            if (path == null) return;

            var resource = resources.Images.SingleOrDefault(e => e.FileName == path);
            if (resource != null)
            {
                resources.Images.Remove(resource);
            }
        }

        public void SetCover(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            RemoveCover();

            var image = Image.FromStream(new MemoryStream(data));
            if (image.RawFormat.Equals(ImageFormat.Png))
            {
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Png);
                    data = stream.ReadToEnd();
                }
            }

            var coverResource = new EpubByteFile
            {
                Content = data,
                FileName = "cover.png",
                ContentType = EpubContentType.ImagePng
            };
            coverResource.MimeType = ContentType.ContentTypeToMimeType[coverResource.ContentType];
            resources.Images.Add(coverResource);

            var coverItem = new OpfManifestItem
            {
                Id = OpfManifest.ManifestItemCoverImageProperty,
                Href = coverResource.FileName,
                MediaType = coverResource.MimeType
            };
            coverItem.Properties.Add(OpfManifest.ManifestItemCoverImageProperty);
            opf.Manifest.Items.Add(coverItem);
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
                    resources.Html.Cast<EpubFile>(),
                    resources.Css,
                    resources.Images,
                    resources.Fonts,
                    resources.Other
                }.SelectMany(collection => collection as EpubFile[] ?? collection.ToArray());
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
