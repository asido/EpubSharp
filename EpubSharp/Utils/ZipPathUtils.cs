namespace EpubSharp.Utils
{
    internal static class ZipPathUtils
    {
        public static string GetDirectoryPath(string filePath)
        {
            var lastSlashIndex = filePath.LastIndexOf('/');
            return lastSlashIndex == -1 ? string.Empty : filePath.Substring(0, lastSlashIndex);
        }

        public static string Combine(string directory, string fileName)
        {
            return string.IsNullOrEmpty(directory) ? fileName : string.Concat(directory, "/", fileName);
        }
    }
}
