using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using EpubSharp.Format;

namespace EpubSharp
{
    public class EpubBook
    {
        /// <summary>
        /// Raw epub format structures. This is populated only when the instance is retrieved using EpubReader.Read()
        /// </summary>
        public EpubFormat Format { get; internal set; }

        public string Title => Format.Package.Metadata.Titles.FirstOrDefault() ?? string.Empty;
        public List<string> Authors => Format.Package.Metadata.Creators.Select(creator => creator.Text).ToList();
        public string Author => string.Join(", ", Authors);
        public EpubResources Resources { get; internal set; }

        internal Lazy<Image> LazyCoverImage = null;
        public Image CoverImage => LazyCoverImage?.Value;

        public List<EpubChapter> Chapters { get; internal set; }

        public string ToPlainText()
        {
            var builder = new StringBuilder();
            foreach (var html in Resources.HtmlInReadingOrder)
            {
                builder.Append(HtmlProcessor.GetContentAsPlainText(html.TextContent));
                builder.Append('\n');
            }
            return builder.ToString().Trim();
        }
    }
    
    public class EpubChapter
    {
        public string Title { get; set; }
        public string ContentFileName { get; set; }
        public string ContentHtml { get; set; }
        public string Anchor { get; set; }
        public List<EpubChapter> SubChapters { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}, Subchapter count: {SubChapters.Count}";
        }
    }
    
    public class EpubResources
    {
        public EpubTextContentFile Ocf { get; internal set; }
        public EpubTextContentFile Opf { get; internal set; }

        public Dictionary<string, EpubTextContentFile> Html { get; internal set; }
        public List<EpubTextContentFile> HtmlInReadingOrder { get; internal set; }

        public Dictionary<string, EpubTextContentFile> Css { get; internal set; }
        public Dictionary<string, EpubByteContentFile> Images { get; internal set; }
        public Dictionary<string, EpubByteContentFile> Fonts { get; internal set; }

        public Dictionary<string, EpubContentFile> AllFiles { get; internal set; }
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
        public string MimeType { get; set; }
        public byte[] Content { get; set; }
    }

    public class EpubByteContentFile : EpubContentFile { }
    
    public class EpubTextContentFile : EpubContentFile
    {
        public string TextContent => Encoding.UTF8.GetString(Content);
    }
}
