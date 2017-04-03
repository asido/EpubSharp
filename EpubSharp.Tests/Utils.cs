using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace EpubSharp.Tests
{
    public static class Utils
    {
        public static List<string> ZipAndCopyEpubs(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var destination = Path.Combine(Cwd.Combine("Samples"), Path.GetFileName(path));
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var samples = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).ToList();
            var archives = new List<string>();

            foreach (var source in samples)
            {
                var archiveName = Path.GetFileName(source) + ".zip";
                var archivePath = Path.Combine(destination, archiveName);
                if (!File.Exists(archivePath))
                {
                    ZipFile.CreateFromDirectory(source, archivePath);
                }
                archives.Add(archivePath);
            }

            return archives;
        }

        public static List<string> CopyEpubs(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var destination = Path.Combine(Cwd.Combine("Samples"), Path.GetFileName(path));
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var samples = Directory.GetFiles(path, "*.epub");
            var archives = new List<string>();

            foreach (var source in samples)
            {
                var archiveName = Path.GetFileName(source);
                var archivePath = Path.Combine(destination, archiveName);
                if (!File.Exists(archivePath))
                {
                    File.Copy(source, archivePath);
                }
                archives.Add(archivePath);
            }

            return archives;
        }
    }
}
