using System.IO;
using System.Linq;
using EpubSharp.Format;
using Xunit;

namespace EpubSharp.Tests
{
    public class EpubWriterTests
    {
        [Fact]
        public void CanWriteTest()
        {
            var book = EpubReader.Read(Cwd.Combine(@"Samples/epub-assorted/Inversions - Iain M. Banks.epub"));
            var writer = new EpubWriter(book);
            writer.Write(new MemoryStream());
        }

        [Fact]
        public void CanCreateEmptyEpubTest()
        {
            var epub = WriteAndRead(new EpubWriter());

            Assert.Null(epub.Title);
            Assert.Equal(0, epub.Authors.Count());
            Assert.Null(epub.CoverImage);

            Assert.Equal(0, epub.Resources.Html.Count);
            Assert.Equal(0, epub.Resources.Css.Count);
            Assert.Equal(0, epub.Resources.Images.Count);
            Assert.Equal(0, epub.Resources.Fonts.Count);
            Assert.Equal(1, epub.Resources.Other.Count); // ncx
            
            Assert.Equal(0, epub.SpecialResources.HtmlInReadingOrder.Count);
            Assert.NotNull(epub.SpecialResources.Ocf);
            Assert.NotNull(epub.SpecialResources.Opf);

            Assert.Equal(0, epub.TableOfContents.Count);

            Assert.NotNull(epub.Format.Ocf);
            Assert.NotNull(epub.Format.Opf);
            Assert.NotNull(epub.Format.Ncx);
            Assert.Null(epub.Format.Nav);
        }

        [Fact]
        public void AddRemoveAuthorTest()
        {
            var writer = new EpubWriter();

            writer.AddAuthor("Foo Bar");
            var epub = WriteAndRead(writer);
            Assert.Equal(1, epub.Authors.Count());

            writer.AddAuthor("Zoo Gar");
            epub = WriteAndRead(writer);
            Assert.Equal(2, epub.Authors.Count());

            writer.RemoveAuthor("Foo Bar");
            epub = WriteAndRead(writer);
            Assert.Equal(1, epub.Authors.Count());
            Assert.Equal("Zoo Gar", epub.Authors.First());

            writer.RemoveAuthor("Unexisting");
            epub = WriteAndRead(writer);
            Assert.Equal(1, epub.Authors.Count());

            writer.ClearAuthors();
            epub = WriteAndRead(writer);
            Assert.Equal(0, epub.Authors.Count());

            writer.RemoveAuthor("Unexisting");
            writer.ClearAuthors();
        }

        [Fact]
        public void AddRemoveTitleTest()
        {
            var writer = new EpubWriter();

            writer.SetTitle("Title1");
            var epub = WriteAndRead(writer);
            Assert.Equal("Title1", epub.Title);

            writer.SetTitle("Title2");
            epub = WriteAndRead(writer);
            Assert.Equal("Title2", epub.Title);

            writer.RemoveTitle();
            epub = WriteAndRead(writer);
            Assert.Null(epub.Title);

            writer.RemoveTitle();
        }

        [Fact]
        public void SetCoverTest()
        {
            var writer = new EpubWriter();
            writer.SetCover(File.ReadAllBytes(Cwd.Combine("Cover.png")), ImageFormat.Png);

            var epub = WriteAndRead(writer);

            Assert.Equal(1, epub.Resources.Images.Count);
            Assert.NotNull(epub.CoverImage);
        }

        [Fact]
        public void RemoveCoverTest()
        {
            var epub1 = EpubReader.Read(Cwd.Combine(@"Samples/epub-assorted/Inversions - Iain M. Banks.epub"));

            var writer = new EpubWriter(EpubWriter.MakeCopy(epub1));
            writer.RemoveCover();

            var epub2 = WriteAndRead(writer);

            Assert.NotNull(epub1.CoverImage);
            Assert.Null(epub2.CoverImage);
            Assert.Equal(epub1.Resources.Images.Count - 1, epub2.Resources.Images.Count);
        }

        [Fact]
        public void RemoveCoverWhenThereIsNoCoverTest()
        {
            var writer = new EpubWriter();
            writer.RemoveCover();
            writer.RemoveCover();
        }

        [Fact]
        public void CanAddChapterTest()
        {
            var writer = new EpubWriter();
            var chapters = new[]
            {
                writer.AddChapter("Chapter 1", "bla bla bla"),
                writer.AddChapter("Chapter 2", "foo bar")
            };
            var epub = WriteAndRead(writer);

            Assert.Equal("Chapter 1", chapters[0].Title);
            Assert.Equal("Chapter 2", chapters[1].Title);

            Assert.Equal(2, epub.TableOfContents.Count);
            for (var i = 0; i < chapters.Length; ++i)
            {
                Assert.Equal(chapters[i].Title, epub.TableOfContents[i].Title);
                Assert.Equal(chapters[i].FileName, epub.TableOfContents[i].FileName);
                Assert.Equal(chapters[i].HashLocation, epub.TableOfContents[i].HashLocation);
                Assert.Equal(0, chapters[i].SubChapters.Count);
                Assert.Equal(0, epub.TableOfContents[i].SubChapters.Count);
            }
        }

        [Fact]
        public void ClearChaptersTest()
        {
            var writer = new EpubWriter();
            writer.AddChapter("Chapter 1", "bla bla bla");
            writer.AddChapter("Chapter 2", "foo bar");
            writer.AddChapter("Chapter 3", "fooz barz");

            var epub = WriteAndRead(writer);
            Assert.Equal(3, epub.TableOfContents.Count);

            writer = new EpubWriter(epub);
            writer.ClearChapters();
            
            epub = WriteAndRead(writer);
            Assert.Equal(0, epub.TableOfContents.Count);
        }

        [Fact]
        public void ClearBogtyvenChaptersTest()
        {
            var writer = new EpubWriter(EpubReader.Read(Cwd.Combine(@"Samples/epub-assorted/bogtyven.epub")));
            writer.ClearChapters();

            var epub = WriteAndRead(writer);
            Assert.Equal(0, epub.TableOfContents.Count);
        }

        [Fact]
        public void AddFileTest()
        {
            var writer = new EpubWriter();
            writer.AddFile("style.css", "body {}", EpubContentType.Css);
            writer.AddFile("img.jpeg", new byte[] { 0x42 }, EpubContentType.ImageJpeg);
            writer.AddFile("font.ttf", new byte[] { 0x24 }, EpubContentType.FontTruetype);

            var epub = WriteAndRead(writer);

            Assert.Equal(1, epub.Resources.Css.Count);
            Assert.Equal("style.css", epub.Resources.Css.First().Href);
            Assert.Equal("body {}", epub.Resources.Css.First().TextContent);

            Assert.Equal(1, epub.Resources.Images.Count);
            Assert.Equal("img.jpeg", epub.Resources.Images.First().Href);
            Assert.Equal(1, epub.Resources.Images.First().Content.Length);
            Assert.Equal(0x42, epub.Resources.Images.First().Content.First());

            Assert.Equal(1, epub.Resources.Fonts.Count);
            Assert.Equal("font.ttf", epub.Resources.Fonts.First().Href);
            Assert.Equal(1, epub.Resources.Fonts.First().Content.Length);
            Assert.Equal(0x24, epub.Resources.Fonts.First().Content.First());
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
