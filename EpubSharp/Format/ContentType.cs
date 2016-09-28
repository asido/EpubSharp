using System.Collections.Generic;
using System.Linq;

namespace EpubSharp.Format
{
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

    internal class ContentType
    {
        public static readonly IReadOnlyDictionary<string, EpubContentType> MimeTypeToContentType = new Dictionary<string, EpubContentType>
        {
            { "application/xhtml+xml", EpubContentType.Xhtml11 },
            { "application/x-dtbook+xml", EpubContentType.Dtbook },
            { "application/x-dtbncx+xml", EpubContentType.DtbookNcx },
            { "text/x-oeb1-document", EpubContentType.Oeb1Document },
            { "application/xml", EpubContentType.Xml },
            { "text/css", EpubContentType.Css },
            { "text/x-oeb1-css", EpubContentType.Oeb1Css },
            { "image/gif", EpubContentType.ImageGif },
            { "image/jpeg", EpubContentType.ImageJpeg },
            { "image/png", EpubContentType.ImagePng },
            { "image/svg+xml", EpubContentType.ImageSvg },
            { "font/truetype", EpubContentType.FontTruetype },
            { "font/opentype", EpubContentType.FontOpentype },
            { "application/vnd.ms-opentype", EpubContentType.FontOpentype }
        };

        public static readonly IReadOnlyDictionary<EpubContentType, string> ContentTypeToMimeType = MimeTypeToContentType
            .Where(pair => pair.Key != "application/vnd.ms-opentype") // Because it's defined twice.
            .ToDictionary(pair => pair.Value, pair => pair.Key);
    }
}
