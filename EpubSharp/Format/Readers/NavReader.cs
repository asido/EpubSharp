using System;
using System.Xml.Linq;

namespace EpubSharp.Format.Readers
{
    internal static class NavReader
    {
        public static NavDocument Read(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException("XML document has no root element.", nameof(xml));

            var nav = new NavDocument
            {
                Head = new NavHead
                {
                    
                },
                Body = new NavBody
                {
                }
            };

            return nav;
        }
    }
}
