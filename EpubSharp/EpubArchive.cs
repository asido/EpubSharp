using System;
using System.IO;
using System.IO.Compression;
using EpubSharp.Format;

namespace EpubSharp
{
    public class EpubArchive
    {
        private readonly ZipArchive archive;

        public EpubArchive(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Specified epub file not found.", filePath);
            }

            archive = Open(File.Open(filePath, FileMode.Open, FileAccess.Read), false);
        }

        public  EpubArchive(byte[] epubData)
        {
            archive = Open(new MemoryStream(epubData), false);
        }

        public EpubArchive(Stream stream, bool leaveOpen)
        {
            archive = Open(stream, leaveOpen);
        }

        private ZipArchive Open(Stream stream, bool leaveOpen)
        {
            return new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen, Constants.DefaultEncoding);
        }

        /// <summary>
        /// Returns an archive entry or null if not found.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ZipArchiveEntry FindEntry(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            return archive.TryGetEntryImproved(path);
        }
    }
}
