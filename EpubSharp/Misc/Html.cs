using System;
using System.Net;
using System.Text.RegularExpressions;

namespace EpubSharp
{
    internal static class Html
    {
        private static readonly RegexOptions RegexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture;
        private static readonly RegexOptions RegexOptionsIgnoreCase = RegexOptions.IgnoreCase | RegexOptions;
        private static readonly RegexOptions RegexOptionsIgnoreCaseSingleLine = RegexOptions.Singleline | RegexOptionsIgnoreCase;
        private static readonly RegexOptions RegexOptionsIgnoreCaseMultiLine = RegexOptions.Multiline | RegexOptionsIgnoreCase;

        public static string GetContentAsPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) throw new ArgumentNullException(nameof(html));

            html = html.Trim();
            html = Regex.Replace(html, @"\r\n?|\n", "");
            var match = Regex.Match(html, @"<body[^>]*>.+</body>", RegexOptionsIgnoreCaseSingleLine);
            return match.Success ? ClearText(match.Value).Trim(' ', '\r', '\n') : "";
        }

        private static string ClearText(string text)
        {
            if (text == null) return null;

            var result = ReplaceBlockTagsWithNewLines(text);
            result = RemoveHtmlTags(result);
            result = DecodeHtmlSymbols(result);
            return result;
        }

        private static string RemoveHtmlTags(string text)
        {
            return text == null ? null : Regex.Replace(text, @"</?(\w+|\s*!--)[^>]*>", " ", RegexOptions);
        }

        private static string ReplaceBlockTagsWithNewLines(string text)
        {
            return text == null ? null : Regex.Replace(text, @"(?<!^\s*)<(p|div|h1|h2|h3|h4|h5|h6)[^>]*>", "\n", RegexOptionsIgnoreCaseMultiLine);
        }

        private static string DecodeHtmlSymbols(string text)
        {
            if (text == null) return null;
            var regex = new Regex(@"(?<defined>(&nbsp|&quot|&mdash|&ldquo|&rdquo|\&\#8211|\&\#8212|&\#8230|\&\#171|&laquo|&raquo|&amp);?)|(?<other>\&\#\d+;?)", RegexOptionsIgnoreCase);
            text = Regex.Replace(regex.Replace(text, SpecialSymbolsEvaluator), @"\ {2,}", " ", RegexOptions);
            text = WebUtility.HtmlDecode(text);
            return text;
        }

        private static string SpecialSymbolsEvaluator(Match m)
        {
            if (!m.Groups["defined"].Success) return " ";
            switch (m.Groups["defined"].Value.ToLower())
            {
                case "&nbsp;": return " ";
                case "&nbsp": return " ";
                case "&quot;": return "\"";
                case "&quot": return "\"";
                case "&mdash;": return " ";
                case "&mdash": return " ";
                case "&ldquo;": return "\"";
                case "&ldquo": return "\"";
                case "&rdquo;": return "\"";
                case "&rdquo": return "\"";
                case "&#8211;": return "-";
                case "&#8211": return "-";
                case "&#8212;": return "-";
                case "&#8212": return "-";
                case "&#8230": return "...";
                case "&#171;": return "\"";
                case "&#171": return "\"";
                case "&laquo;": return "\"";
                case "&laquo": return "\"";
                case "&raquo;": return "\"";
                case "&raquo": return "\"";
                case "&amp;": return "&";
                case "&amp": return "&";
                default: return " ";
            }
        }
    }
}
