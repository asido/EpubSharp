using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EpubSharp.Tests
{
    [TestClass]
    public class EpubWriterTests
    {
        [TestMethod]
        public void SaveTest()
        {
            var book = EpubReader.Read(@"../../Samples/epub-assorted/iOS Hackers Handbook.epub");
            var writer = new EpubWriter(book);
            writer.Save("saved.epub");
        }
    }
}
