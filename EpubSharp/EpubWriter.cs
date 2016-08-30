using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        private byte[] coverData; // TODO: Replace this eventually with EpubResources object.

        public EpubWriter() { }

        public EpubWriter(EpubBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (book.Format?.Opf == null) throw new ArgumentException("book opf instance == null", nameof(book));

            opf = book.Format.Opf;
        }

        public void SetCover(byte[] image, ImageFormat format)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var coverItem = new OpfManifestItem { Id = "cover-image" };
            switch (format)
            {
                case ImageFormat.Jpeg:
                    coverItem.Href = "cover.jpg";
                    break;
                case ImageFormat.Png:
                    coverItem.Href = "cover.png";
                    break;
                default:
                    throw new ArgumentException($"Unknown format: {format}", nameof(format));
            }
            opf.Manifest.Items.Add(coverItem);

            coverData = image;
        }

        public void AddAuthor(string author)
        {
            if (author == null) throw new ArgumentNullException(nameof(author));
            opf.Metadata.Creators.Add(new OpfMetadataCreator { Text = author });
        }

        public void Save(string filename)
        {
            using (var file = File.Create(filename))
            {
                Save(file);
            }
        }

        public void Save(Stream stream)
        {
            var archive = new ZipArchive(stream, ZipArchiveMode.Create);
            archive.CreateEntry("mimetype", MimeTypeWriter.Format());
            archive.CreateEntry(Constants.OcfPath, OcfWriter.Format(OpfPath));
            archive.CreateEntry(OpfPath, OpfWriter.Format(opf));
            archive.Dispose();
        }
    }
}
