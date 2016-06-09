using System.IO.Compression;
using EpubSharp.Format;
using EpubSharp.Schema.Navigation;
using EpubSharp.Schema.Opf;
using EpubSharp.Utils;

namespace EpubSharp.Readers
{
    internal static class SchemaReader
    {
        public static PackageDocument ReadSchema(ZipArchive epubArchive)
        {
            var result = new PackageDocument();
            string rootFilePath = RootFilePathReader.GetRootFilePath(epubArchive);
            string contentDirectoryPath = ZipPathUtils.GetDirectoryPath(rootFilePath);
            result.ContentDirectoryPath = contentDirectoryPath;
            EpubPackage package = PackageReader.ReadPackage(epubArchive, rootFilePath);
            result.Package = package;
            EpubNavigation navigation = NavigationReader.ReadNavigation(epubArchive, contentDirectoryPath, package);
            result.Navigation = navigation;
            return result;
        }
    }
}
