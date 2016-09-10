using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EpubSharp.Format;
using HtmlAgilityPack;

namespace EpubSharp
{
    internal static class ZipArchiveExt
    {
        public static void CreateEntry(this ZipArchive archive, string file, string content)
        {
            var data = Encoding.UTF8.GetBytes(content);
            archive.CreateEntry(file, data);
        }

        public static void CreateEntry(this ZipArchive archive, string file, byte[] data)
        {
            var entry = archive.CreateEntry(file);
            using (var stream = entry.Open())
            {
                stream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// ZIP's are slash-side sensitive and ZIP's created on Windows and Linux can contain their own variation.
        /// </summary>
        public static ZipArchiveEntry GetEntryIgnoringSlashDirection(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntry(entryName);

            if (entry == null)
            {
                var namesToTry = new List<string>();
                
                // I've seen epubs, where manifest href's are url encoded, but files in archive not.
                namesToTry.Add(Uri.UnescapeDataString(entryName));

                // Such epubs aren't common, but zip archives created on windows uses backslashes.
                // That could happen if an epub is re-archived manually.
                foreach (var newName in new[]
                {
                    entryName.Replace(@"\", @"/"),
                    entryName.Replace("/", @"\")
                }.Where(newName => newName != entryName))
                {
                    namesToTry.Add(newName);
                    namesToTry.Add(Uri.UnescapeDataString(newName));
                }

                foreach (var newName in namesToTry)
                {
                    entry = archive.GetEntry(newName);
                    if (entry != null)
                    {
                        break;
                    }
                }
            }

            if (entry == null)
            {
                throw new EpubParseException($"{entryName} file not found in archive.");
            }

            return entry;
        }
        
        public static byte[] LoadBytes(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntryIgnoringSlashDirection(entryName);
            using (var stream = entry.Open())
            {
                var data = stream.ReadToEnd();
                return data;
            }
        }

        public static string LoadText(this ZipArchive archive, string entryName)
        {
            var data = archive.LoadBytes(entryName);
            var str = Encoding.UTF8.GetString(data);
            return str;
        }

        public static XDocument LoadXml(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntryIgnoringSlashDirection(entryName);

            using (var stream = entry.Open())
            {
                var xml = XDocument.Load(stream);
                return xml;
            }
        }

        public static XDocument LoadHtml(this ZipArchive archive, string entryName)
        {
            var html = archive.LoadText(entryName);
            html = html.Trim();

            // Strip everything above doctype, because some navigation HTMLs start with <?xml ... ?> declaration.
            // It's a loop, because some EPUBs have multiple declarations. I.e.:
            /*
                <?xml version="1.0" encoding="UTF-8"?>
                <?xml-model href="file:/C:/EPub/epub-revision/build/30/schema/epub-nav-30.rnc" type="application/relax-ng-compact-syntax"?>
             */
            while (html.StartsWith(Constants.XmlDeclarationPrefix))
            {
                var declarationEnd = html.IndexOf(Constants.XmlDeclarationSufix, StringComparison.Ordinal);
                if (declarationEnd == -1)
                {
                    throw new InvalidOperationException("HTML starts with an XML declaration, but couldn't find the end.");
                }

                html = html.Substring(declarationEnd + Constants.XmlDeclarationSufix.Length);
                html = html.Trim();
            }

            var doc = new HtmlDocument { OptionWriteEmptyNodes = true };
            doc.LoadHtml(html);

            using (var stream = new MemoryStream())
            {
                doc.Save(stream);
                try
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var xml = Encoding.UTF8.GetString(stream.ReadToEnd());
                    return XDocument.Parse(xml);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
