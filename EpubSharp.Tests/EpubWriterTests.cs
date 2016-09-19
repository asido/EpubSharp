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
        public void AddRemoveAuthorTest()
        {
            var writer = new EpubWriter();

            writer.AddAuthor("Foo Bar");
            var epub = WriteAndRead(writer);
            Assert.AreEqual(1, epub.Authors.Count);

            writer.AddAuthor("Zoo Gar");
            epub = WriteAndRead(writer);
            Assert.AreEqual(2, epub.Authors.Count);

            writer.RemoveAuthor("Foo Bar");
            epub = WriteAndRead(writer);
            Assert.AreEqual(1, epub.Authors.Count);
            Assert.AreEqual("Zoo Gar", epub.Authors[0]);

            writer.RemoveAuthor("Unexisting");
            epub = WriteAndRead(writer);
            Assert.AreEqual(1, epub.Authors.Count);

            writer.ClearAuthors();
            epub = WriteAndRead(writer);
            Assert.AreEqual(0, epub.Authors.Count);

            writer.RemoveAuthor("Unexisting");
            writer.ClearAuthors();
        }

        [TestMethod]
        public void AddRemoveTitleTest()
        {
            var writer = new EpubWriter();

            writer.SetTitle("Title1");
            var epub = WriteAndRead(writer);
            Assert.AreEqual("Title1", epub.Title);

            writer.SetTitle("Title2");
            epub = WriteAndRead(writer);
            Assert.AreEqual("Title2", epub.Title);

            writer.RemoveTitle();
            epub = WriteAndRead(writer);
            Assert.IsNull(epub.Title);

            writer.RemoveTitle();
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

            var writer = new EpubWriter(EpubWriter.MakeCopy(epub1));
            writer.RemoveCover();

            var epub2 = WriteAndRead(writer);

            Assert.IsNotNull(epub1.CoverImage);
            Assert.IsNull(epub2.CoverImage);
            Assert.AreEqual(epub1.Resources.Images.Count - 1, epub2.Resources.Images.Count);
        }

        [TestMethod]
        public void RemoveCoverWhenThereIsNoCoverTest()
        {
            var writer = new EpubWriter();
            writer.RemoveCover();
            writer.RemoveCover();
        }

        [TestMethod]
        public void CanAddChapterTest()
        {
            var writer = new EpubWriter();
            var chapters = new[]
            {
                writer.AddChapter("Chapter 1", "bla bla bla"),
                writer.AddChapter("Chapter 2", "foo bar")
            };
            var epub = WriteAndRead(writer);

            Assert.AreEqual("Chapter 1", chapters[0].Title);
            Assert.AreEqual("Chapter 2", chapters[1].Title);

            Assert.AreEqual(2, epub.TableOfContents.Count);
            for (var i = 0; i < chapters.Length; ++i)
            {
                Assert.AreEqual(chapters[i].Title, epub.TableOfContents[i].Title);
                Assert.AreEqual(chapters[i].FileName, epub.TableOfContents[i].FileName);
                Assert.AreEqual(chapters[i].Anchor, epub.TableOfContents[i].Anchor);
                Assert.AreEqual(0, chapters[i].SubChapters.Count);
                Assert.AreEqual(0, epub.TableOfContents[i].SubChapters.Count);
            }
        }

        [TestMethod]
        public void ClearChaptersTest()
        {
            var writer = new EpubWriter();
            writer.AddChapter("Chapter 1", "bla bla bla");
            writer.AddChapter("Chapter 2", "foo bar");
            writer.AddChapter("Chapter 3", "fooz barz");

            var epub = WriteAndRead(writer);
            Assert.AreEqual(3, epub.TableOfContents.Count);

            writer = new EpubWriter(epub);
            writer.ClearChapters();

            var epub1 = EpubReader.Read(@"../../Samples/epub-assorted/bogtyven.epub");
            var bogtyven = new EpubWriter(epub1);
            bogtyven.ClearChapters();
            bogtyven.Write(@"D:\bogtyven.epub");

            epub = WriteAndRead(writer);
            Assert.AreEqual(0, epub.TableOfContents.Count);
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
