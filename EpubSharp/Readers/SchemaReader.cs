using System.IO.Compression;
using EpubSharp.Format;
using EpubSharp.Utils;

namespace EpubSharp.Readers
{
    internal static class SchemaReader
    {
        public static PackageDocument ReadSchema(ZipArchive epubArchive)
        {
            var rootFilePath = RootFilePathReader.GetRootFilePath(epubArchive);
            var contentDirectoryPath = ZipPathUtils.GetDirectoryPath(rootFilePath);

            var package = PackageReader.ReadPackage(epubArchive, rootFilePath);
            package.ContentDirectoryPath = contentDirectoryPath;
            package.Navigation = NavigationReader.ReadNavigation(epubArchive, contentDirectoryPath, package);
            return package;
        }
    }
}
