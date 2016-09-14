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
        internal const string AuthorsSeparator = ", ";

        /// <summary>
        /// Read-only raw epub format structures.
        /// </summary>
        public EpubFormat Format { get; internal set; }

        public string Title => Format.Opf.Metadata.Titles.FirstOrDefault() ?? string.Empty;
        public List<string> Authors => Format.Opf.Metadata.Creators.Select(creator => creator.Text).ToList();
        public string Author => string.Join(AuthorsSeparator, Authors);
        public EpubResources Resources { get; internal set; }
        public EpubSpecialResources SpecialResources { get; internal set; }

        internal Lazy<Image> LazyCoverImage = null;
        public Image CoverImage => LazyCoverImage?.Value;

        public List<EpubChapter> TableOfContents { get; internal set; }

        public string ToPlainText()
        {
            var builder = new StringBuilder();
            foreach (var html in SpecialResources.HtmlInReadingOrder)
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
        public string FileName { get; set; }
        public string Anchor { get; set; }
        public List<EpubChapter> SubChapters { get; set; } = new List<EpubChapter>();

        public override string ToString()
        {
            return $"Title: {Title}, Subchapter count: {SubChapters.Count}";
        }
    }

    public class EpubResources
    {
        internal List<EpubTextFile> html = new List<EpubTextFile>();
        public IReadOnlyCollection<EpubTextFile> Html => html;

        internal List<EpubTextFile> css = new List<EpubTextFile>();
        public IReadOnlyCollection<EpubTextFile> Css => css;

        internal List<EpubByteFile> images = new List<EpubByteFile>();
        public IReadOnlyCollection<EpubByteFile> Images => images;

        internal List<EpubByteFile> fonts = new List<EpubByteFile>();
        public IReadOnlyCollection<EpubByteFile> Fonts => fonts;

        internal List<EpubFile> other = new List<EpubFile>();
        public IReadOnlyCollection<EpubFile> Other => other;
    }

    public class EpubSpecialResources
    {
        public EpubTextFile Ocf { get; internal set; }
        public EpubTextFile Opf { get; internal set; }
        public List<EpubTextFile> HtmlInReadingOrder { get; internal set; } = new List<EpubTextFile>();
    }

    public abstract class EpubFile
    {
        public string FileName { get; set; }
        public EpubContentType ContentType { get; set; }
        public string MimeType { get; set; }
        public byte[] Content { get; set; }
    }

    public class EpubByteFile : EpubFile { }
    
    public class EpubTextFile : EpubFile
    {
        public string TextContent => Encoding.UTF8.GetString(Content);
    }
}
