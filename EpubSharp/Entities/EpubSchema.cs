using EpubSharp.Schema.Navigation;
using EpubSharp.Schema.Opf;

namespace EpubSharp.Entities
{
    public class EpubSchema
    {
        public EpubPackage Package { get; set; }
        public EpubNavigation Navigation { get; set; }
        public string ContentDirectoryPath { get; set; }
    }
}
