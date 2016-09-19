using System;

namespace EpubSharp
{
    internal class Href
    {
        public readonly string Filename;
        public readonly string Anchor;

        public Href(string href)
        {
            if (string.IsNullOrWhiteSpace(href)) throw new ArgumentNullException(nameof(href));

            var contentSourceAnchorCharIndex = href.IndexOf('#');
            if (contentSourceAnchorCharIndex == -1)
            {
                Filename = href;
            }
            else
            {
                Filename = href.Substring(0, contentSourceAnchorCharIndex);
                Anchor = href.Substring(contentSourceAnchorCharIndex + 1);
            }
        }
    }
}
