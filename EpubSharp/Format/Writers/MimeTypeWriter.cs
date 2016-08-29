using System;
using System.IO.Compression;

namespace EpubSharp.Format.Writers
{
    internal class MimeTypeWriter
    {
        public static void Write(ZipArchive archive)
        {
            archive.CreateEntry("mimetype", "application/epub+zip");
        }
    }
}
