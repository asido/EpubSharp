using System;

namespace EpubSharp.Format.Writers
{
    internal class OcfWriter
    {
        public static string Format(string opfPath)
        {
            if (string.IsNullOrWhiteSpace(opfPath)) throw new ArgumentNullException(nameof(opfPath));

            return @"<?xml version=""1.0""?>
<container version=""1.0"" xmlns=""urn:oasis:names:tc:opendocument:xmlns:container"">
  <rootfiles>
    <rootfile full-path=""" + opfPath + @""" media-type = ""application/oebps-Opf+xml"" />
  </rootfiles>
</container>";
        }
    }
}
