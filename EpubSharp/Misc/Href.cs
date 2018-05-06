using System;

namespace EpubSharp
{
    internal class Href
    {
        public readonly string Path;
        public readonly string HashLocation;

        public Href(string href)
        {
            if (string.IsNullOrWhiteSpace(href)) throw new ArgumentNullException(nameof(href));

            var contentSourceAnchorCharIndex = href.IndexOf('#');
            if (contentSourceAnchorCharIndex == -1)
            {
                Path = href;
            }
            else
            {
                Path = href.Substring(0, contentSourceAnchorCharIndex);
                HashLocation = href.Substring(contentSourceAnchorCharIndex + 1);
            }
        }
    }
}
