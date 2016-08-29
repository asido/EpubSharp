using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using EpubSharp.Format.Writers;

namespace EpubSharp
{
    public enum ImageFormat
    {
        Jpeg, Png
    }

    public class EpubWriter
    {
        private readonly List<string> authors = new List<string>();

        private string coverFilename;
        private byte[] coverData;

        public EpubWriter() { }

        public EpubWriter(EpubBook book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));

            authors.AddRange(book.Authors);

            EpubByteContentFile cover;
            if (book.Resources.Images.TryGetValue(book.Format.Opf.CoverPath, out cover))
            {
                coverFilename = Path.GetFileName(cover.FileName);
                coverData = cover.Content;
            }
        }

        public void SetCover(byte[] image, ImageFormat format)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            switch (format)
            {
                case ImageFormat.Jpeg:
                    coverFilename = "cover.jpg";
                    break;
                case ImageFormat.Png:
                    coverFilename = "cover.png";
                    break;
                default:
                    throw new ArgumentException($"Unknown format: {format}", nameof(format));
            }

            coverData = image;
        }

        public void AddAuthor(string author)
        {
            if (author == null) throw new ArgumentNullException(nameof(author));
            authors.Add(author);
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
            MimeTypeWriter.Write(archive);
            OcfWriter.Write(archive);
            archive.Dispose();
        }
    }
}
