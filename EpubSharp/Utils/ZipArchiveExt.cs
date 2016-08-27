using System.IO.Compression;
using System.Linq;
using System.Xml;

namespace EpubSharp
{
    internal static class ZipArchiveExt
    {
        /// <summary>
        /// ZIP's are slash-side sensitive and ZIP's created on Windows and Linux can contain their own variation.
        /// </summary>
        public static ZipArchiveEntry GetEntryIgnoringSlashDirection(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntry(entryName);

            if (entry == null)
            {
                foreach (var newName in new[]
                {
                    entryName.Replace(@"\", @"/"),
                    entryName.Replace("/", @"\")
                }.Where(newName => newName != entryName))
                {
                    entry = archive.GetEntry(newName);
                }
            }

            if (entry == null)
            {
                throw new EpubException($"EPUB parsing error: {entryName} file not found in archive.");
            }

            return entry;
        }

        public static XmlDocument LoadXml(this ZipArchive archive, string entryName)
        {
            var containerFileEntry = archive.GetEntryIgnoringSlashDirection(entryName);
            using (var containerStream = containerFileEntry.Open())
            {
                return XmlExt.LoadDocument(containerStream);
            }
        }
    }
}
