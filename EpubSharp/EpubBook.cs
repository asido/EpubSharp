using System.Collections.Generic;
using System.Drawing;
using EpubSharp.Schema.Navigation;
using EpubSharp.Schema.Opf;

namespace EpubSharp
{
    public class EpubBook
    {
        public string FilePath { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public List<string> AuthorList { get; set; }
        public EpubSchema Schema { get; set; }
        public EpubContent Content { get; set; }
        public Image CoverImage { get; set; }
        public List<EpubChapter> Chapters { get; set; }
    }

    public class EpubSchema
    {
        public EpubPackage Package { get; set; }
        public EpubNavigation Navigation { get; set; }
        public string ContentDirectoryPath { get; set; }
    }

    public class EpubChapter
    {
        public string Title { get; set; }
        public string ContentFileName { get; set; }
        public string Anchor { get; set; }
        public string HtmlContent { get; set; }
        public List<EpubChapter> SubChapters { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}, Subchapter count: {SubChapters.Count}";
        }
    }
    
    public class EpubContent
    {
        public Dictionary<string, EpubTextContentFile> Html { get; set; }
        public Dictionary<string, EpubTextContentFile> Css { get; set; }
        public Dictionary<string, EpubByteContentFile> Images { get; set; }
        public Dictionary<string, EpubByteContentFile> Fonts { get; set; }
        public Dictionary<string, EpubContentFile> AllFiles { get; set; }
    }

    public enum EpubContentType
    {
        Xhtml11 = 1,
        Dtbook,
        DtbookNcx,
        Oeb1Document,
        Xml,
        Css,
        Oeb1Css,
        ImageGif,
        ImageJpeg,
        ImagePng,
        ImageSvg,
        FontTruetype,
        FontOpentype,
        Other
    }

    public abstract class EpubContentFile
    {
        public string FileName { get; set; }
        public EpubContentType ContentType { get; set; }
        public string ContentMimeType { get; set; }
        public byte[] Content { get; set; }
    }

    public class EpubByteContentFile : EpubContentFile
    {
        public new byte[] Content { get; set; }
    }
    
    public class EpubTextContentFile : EpubContentFile
    {
        public new string Content { get; set; }
    }
}
