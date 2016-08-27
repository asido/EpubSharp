using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

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
                throw new EpubParseException($"{entryName} file not found in archive.");
            }

            return entry;
        }

        public static XmlDocument LoadXml(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntryIgnoringSlashDirection(entryName);
            using (var stream = entry.Open())
            {
                return XmlExt.LoadDocument(stream);
            }
        }

        public static XDocument LoadXDocument(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntryIgnoringSlashDirection(entryName);

            using (var stream = entry.Open())
            {
                return XDocument.Load(stream);
            }
        }
    }
}
