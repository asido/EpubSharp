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
    public class EpubWriterTests
    {
        [TestMethod]
        public void CanWriteTest()
        {
            var book = EpubReader.Read(@"../../Samples/epub-assorted/Inversions - Iain M. Banks.epub");
            var writer = new EpubWriter(book);
            writer.Write(new MemoryStream());
        }

        [TestMethod]
        public void CanCreateEmptyEpubTest()
        {
            var epub = WriteAndRead(new EpubWriter());

            Assert.IsNull(epub.Title);
            Assert.IsNull(epub.Author);
            Assert.AreEqual(0, epub.Authors.Count);
            Assert.IsNull(epub.CoverImage);

            Assert.AreEqual(0, epub.Resources.Html.Count);
            Assert.AreEqual(0, epub.Resources.Css.Count);
            Assert.AreEqual(0, epub.Resources.Images.Count);
            Assert.AreEqual(0, epub.Resources.Fonts.Count);
            Assert.AreEqual(1, epub.Resources.Other.Count); // ncx
            
            Assert.AreEqual(0, epub.SpecialResources.HtmlInReadingOrder.Count);
            Assert.IsNotNull(epub.SpecialResources.Ocf);
            Assert.IsNotNull(epub.SpecialResources.Opf);

            Assert.AreEqual(0, epub.TableOfContents.Count);

            Assert.IsNotNull(epub.Format.Ocf);
            Assert.IsNotNull(epub.Format.Opf);
            Assert.IsNotNull(epub.Format.Ncx);
            Assert.IsNull(epub.Format.Nav);
        }

        [TestMethod]
        public void SetCoverTest()
        {
            var writer = new EpubWriter();
            writer.SetCover(File.ReadAllBytes("Cover.png"));

            var epub = WriteAndRead(writer);

            Assert.AreEqual(1, epub.Resources.Images.Count);
            Assert.IsNotNull(epub.CoverImage);
        }

        [TestMethod]
        public void RemoveCoverTest()
        {
            var epub1 = EpubReader.Read(@"../../Samples/epub-assorted/Inversions - Iain M. Banks.epub");
            var epub1ImageCount = epub1.Resources.Images.Count;

            var writer = new EpubWriter(epub1);
            writer.RemoveCover();

            var epub2 = WriteAndRead(writer);

            Assert.IsNotNull(epub1.CoverImage);
            Assert.IsNull(epub2.CoverImage);
            Assert.AreEqual(epub1ImageCount - 1, epub2.Resources.Images.Count);
        }

        private EpubBook WriteAndRead(EpubWriter writer)
        {
            var stream = new MemoryStream();
            writer.Write(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var epub = EpubReader.Read(stream, false);
            return epub;
        }
    }
}
