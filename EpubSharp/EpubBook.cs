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

        public string Title => Format.Opf.Metadata.Titles.FirstOrDefault();
        public IList<string> Authors => Format.Opf.Metadata.Creators.Select(creator => creator.Text).ToList();
        public string Author => Authors.Any() ? string.Join(AuthorsSeparator, Authors) : null;
        public EpubResources Resources { get; internal set; }
        public EpubSpecialResources SpecialResources { get; internal set; }
        public byte[] CoverImage { get; internal set; }
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
        public IList<EpubChapter> SubChapters { get; set; } = new List<EpubChapter>();

        public override string ToString()
        {
            return $"Title: {Title}, Subchapter count: {SubChapters.Count}";
        }
    }

    public class EpubResources
    {
        public ICollection<EpubTextFile> Html { get; internal set; } = new List<EpubTextFile>();
        public ICollection<EpubTextFile> Css { get; internal set; } = new List<EpubTextFile>();
        public ICollection<EpubByteFile> Images { get; internal set; } = new List<EpubByteFile>();
        public ICollection<EpubByteFile> Fonts { get; internal set; } = new List<EpubByteFile>();
        public ICollection<EpubFile> Other { get; internal set; } = new List<EpubFile>();
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
