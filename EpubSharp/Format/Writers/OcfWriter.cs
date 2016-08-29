using System.IO.Compression;

namespace EpubSharp.Format.Writers
{
    internal class OcfWriter
    {
        public static void Write(ZipArchive archive)
        {
            archive.CreateEntry(Constants.OcfPath, @"<?xml version=""1.0""?>
<container version=""1.0"" xmlns=""urn:oasis:names:tc:opendocument:xmlns:container"">
  <rootfiles>
    <rootfile full-path=""EPUB/Opf.opf"" media-type = ""application/oebps-Opf+xml"" />
  </rootfiles>
</container>");
        }
    }
}
