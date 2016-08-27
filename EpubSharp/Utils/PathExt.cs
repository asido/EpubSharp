using System;

namespace EpubSharp.Utils
{
    internal static class PathExt
    {
        public static string GetDirectoryPath(string filePath)
        {
            var lastSlashIndex = filePath.LastIndexOf('/');
            var dir = lastSlashIndex == -1 ? string.Empty : filePath.Substring(0, lastSlashIndex);
            if (dir == "/")
            {
                dir = "";
            }
            return dir;
        }

        public static string Combine(string directory, string fileName)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return fileName;
            }

            if (directory.EndsWith("/"))
            {
                directory = directory.Substring(0, directory.Length - 1);
            }

            while (fileName.StartsWith("../"))
            {
                var newDir = GetDirectoryPath(directory);
                if (newDir == directory)
                {
                    throw new InvalidOperationException($"There is no room to normalize '../'. Directory={directory}, filename={fileName}");
                }
                directory = newDir;
                fileName = fileName.Substring(3);
            }

            return string.IsNullOrEmpty(directory) ? fileName : string.Concat(directory, "/", fileName);
        }
    }
}
