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
                var originalEpub = EpubReader.Read(archive);

                var stream = new MemoryStream();
                EpubWriter.Write(originalEpub, stream);
                stream.Seek(0, SeekOrigin.Begin);
                var savedEpub = EpubReader.Read(stream, false);

                AssertEpub(originalEpub, savedEpub);
            }
        }

        private void AssertEpub(EpubBook expected, EpubBook actual)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Title, actual.Title);

            Assert.AreEqual(expected.Author, actual.Author);
            AssertPrimitiveCollection(expected.Authors, actual.Authors, nameof(actual.Authors), "Author");

            Assert.AreEqual(expected.CoverImage == null, actual.CoverImage == null, nameof(actual.CoverImage));
            if (expected.CoverImage != null && actual.CoverImage != null)
            {
                Assert.AreEqual(expected.CoverImage.Height, actual.CoverImage.Height, "CoverImage.Height");
                Assert.AreEqual(expected.CoverImage.Width, actual.CoverImage.Width, "CoverImage.Width");
            }
        }

        private void AssertCollection<T>(IEnumerable<T> expected, IEnumerable<T> actual, string name, Action<List<T>, List<T>, int> assert)
        {
            Assert.AreEqual(expected == null, actual == null, name);
            if (expected != null && actual != null)
            {
                var old = expected.ToList();
                var @new = actual.ToList();

                Assert.AreEqual(old.Count, @new.Count, $"{name}.Count");

                for (var i = 0; i < @new.Count; ++i)
                {
                    assert(old, @new, i);
                }
            }
        }

        private void AssertPrimitiveCollection<T>(IEnumerable<T> expected, IEnumerable<T> actual, string collectionName, string unitName)
        {
            AssertCollection(expected, actual, collectionName, (old, @new, i) =>
            {
                Assert.IsTrue(@new.Contains(old[i]), unitName);
            });
        }
    }
}
