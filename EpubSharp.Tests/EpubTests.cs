using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EpubSharp.Tests
{
    [TestClass]
    public class EpubTests
    {
        [TestMethod]
        public void ReadWriteEpub30Test()
        {
            var archives = Utils.ZipAndCopyEpubs(@"../../Samples/epub30");
            ReadWriteTest(archives);
        }

        [TestMethod]
        public void ReadWriteEpub31Test()
        {
            var archives = Utils.ZipAndCopyEpubs(@"../../Samples/epub31");
            ReadWriteTest(archives);
        }

        [TestMethod]
        public void ReadWriteEpubAssortedTest()
        {
            var archives = Utils.ZipAndCopyEpubs(@"../../Samples/epub-assorted");
            ReadWriteTest(archives);
        }

        private void ReadWriteTest(List<string> archives)
        {
            foreach (var archive in archives)
            {
                var origEpub = EpubReader.Read(archive);

                var stream = new MemoryStream();
                EpubWriter.Write(origEpub, stream);
                stream.Seek(0, SeekOrigin.Begin);
                var savedEpub = EpubReader.Read(stream, false);

                // TODO: Do asserts to compare the original and the saved epubs.
            }
        }
    }
}
