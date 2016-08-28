using System;
using System.Net;
using System.Text.RegularExpressions;

namespace EpubSharp
{
    internal static class HtmlProcessor
    {
        private static readonly RegexOptions REO_ = RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture;
        private static readonly RegexOptions REO_c = RegexOptions.Compiled | REO_;
        private static readonly RegexOptions REO_i = RegexOptions.IgnoreCase | REO_;
        private static readonly RegexOptions REO_ci = RegexOptions.IgnoreCase | REO_c;
        private static readonly RegexOptions REO_si = RegexOptions.Singleline | REO_i;
        private static readonly RegexOptions REO_csi = RegexOptions.Compiled | REO_si;
        private static readonly RegexOptions REO_mi = RegexOptions.Multiline | REO_i;
        private static readonly RegexOptions REO_cmi = RegexOptions.Compiled | REO_mi;

        public static string GetContentAsPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) throw new ArgumentNullException(nameof(html));

            html = html.Trim();
            html = Regex.Replace(html, @"\r\n?|\n", "");
            var match = Regex.Match(html, @"<body[^>]*>.+</body>", REO_csi);
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
            return text == null ? null : Regex.Replace(text, @"</?(\w+|\s*!--)[^>]*>", " ", REO_c);
        }

        private static string ReplaceBlockTagsWithNewLines(string text)
        {
            return text == null ? null : Regex.Replace(text, @"(?<!^\s*)<(p|div|h1|h2|h3|h4|h5|h6)[^>]*>", "\n", REO_cmi);
        }

        private static string DecodeHtmlSymbols(string text)
        {
            if (text == null) return null;
            var regex = new Regex(@"(?<defined>(&nbsp|&quot|&mdash|&ldquo|&rdquo|\&\#8211|\&\#8212|&\#8230|\&\#171|&laquo|&raquo|&amp);?)|(?<other>\&\#\d+;?)", REO_ci);
            text = Regex.Replace(regex.Replace(text, SpecialSymbolsEvaluator), @"\ {2,}", " ", REO_c);
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
