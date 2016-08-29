using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            WriteMimeType(archive);
            WriteOcf(archive);
            archive.Dispose();
        }

        private void WriteMimeType(ZipArchive archive)
        {
            Write(archive, "mimetype", "application/epub+zip");
        }

        private void WriteOcf(ZipArchive archive)
        {
            Write(archive, EpubReader.OcfPath, @"<?xml version=""1.0""?>
<container version=""1.0"" xmlns=""urn:oasis:names:tc:opendocument:xmlns:container"">
  <rootfiles>
    <rootfile full-path=""EPUB/Opf.opf"" media-type = ""application/oebps-Opf+xml"" />
  </rootfiles>
</container>");
        }

        private void Write(ZipArchive archive, string file, string content)
        {
            var entry = archive.CreateEntry(file);
            using (var stream = new StreamWriter(entry.Open()))
            {
                stream.Write(content);
            }
        }
    }
}
