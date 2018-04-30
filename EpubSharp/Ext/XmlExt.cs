using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EpubSharp
{
    internal static class XmlExt
    {
        public static ICollection<string> AsStringList(this IEnumerable<XElement> self)
        {
            return self.Select(elem => elem.Value).ToList();
        }

        public static ICollection<T> AsObjectList<T>(this IEnumerable<XElement> self, Func<XElement, T> factory)
        {
            return self.Select(factory).Where(value => value != null).ToList();
        }
    }
}
