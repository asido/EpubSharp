using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpubSharp.Format;
using FluentAssertions;
using Xunit;

namespace EpubSharp.Tests
{
    public class EpubReaderTests
    {
        [Fact]
        public void ReadBogtyvenFormatTest()
        {
            var book = EpubReader.Read(Cwd.Combine(@"Samples/Bogtyven.epub"));
            var format = book.Format;

            Assert.NotNull(format);

            Assert.NotNull(format.Ocf);
            Assert.Equal(1, format.Ocf.RootFiles.Count);
            Assert.Equal("OPS/9788711332412.opf", format.Ocf.RootFiles.ElementAt(0).FullPath);
            Assert.Equal("application/oebps-package+xml", format.Ocf.RootFiles.ElementAt(0).MediaType);
            Assert.Equal("OPS/9788711332412.opf", format.Ocf.RootFilePath);

            Assert.NotNull(format.Opf);
            Assert.Equal("ISBN9788711332412", format.Opf.UniqueIdentifier);
            Assert.Equal(EpubVersion.Epub3, format.Opf.EpubVersion);

            /*
            <guide>
                <reference type="cover" href="xhtml/cover.xhtml"/>
                <reference type="title-page" href="xhtml/title.xhtml"/>
                <reference type="chapter" href="xhtml/prologue.xhtml"/>
                <reference type="copyright-page" href="xhtml/copyright.xhtml"/>
            </guide>
             */
            Assert.NotNull(format.Opf.Guide);
            Assert.Equal(4, format.Opf.Guide.References.Count);

            Assert.Equal("xhtml/cover.xhtml", format.Opf.Guide.References.ElementAt(0).Href);
            Assert.Equal(null, format.Opf.Guide.References.ElementAt(0).Title);
            Assert.Equal("cover", format.Opf.Guide.References.ElementAt(0).Type);

            Assert.Equal("xhtml/title.xhtml", format.Opf.Guide.References.ElementAt(1).Href);
            Assert.Equal(null, format.Opf.Guide.References.ElementAt(1).Title);
            Assert.Equal("title-page", format.Opf.Guide.References.ElementAt(1).Type);

            Assert.Equal("xhtml/prologue.xhtml", format.Opf.Guide.References.ElementAt(2).Href);
            Assert.Equal(null, format.Opf.Guide.References.ElementAt(2).Title);
            Assert.Equal("chapter", format.Opf.Guide.References.ElementAt(2).Type);

            Assert.Equal("xhtml/copyright.xhtml", format.Opf.Guide.References.ElementAt(3).Href);
            Assert.Equal(null, format.Opf.Guide.References.ElementAt(3).Title);
            Assert.Equal("copyright-page", format.Opf.Guide.References.ElementAt(3).Type);
                      
            Assert.NotNull(format.Opf.Manifest);
            Assert.Equal(150, format.Opf.Manifest.Items.Count);

            // <item id="body097" href="xhtml/chapter_083.xhtml" media-type="application/xhtml+xml" properties="svg"/>
            var item = format.Opf.Manifest.Items.First(e => e.Id == "body097");
            Assert.Equal("xhtml/chapter_083.xhtml", item.Href);
            Assert.Equal("application/xhtml+xml", item.MediaType);
            Assert.Equal(1, item.Properties.Count);
            Assert.Equal("svg", item.Properties.ElementAt(0));
            Assert.Null(item.Fallback);
            Assert.Null(item.FallbackStyle);
            Assert.Null(item.RequiredModules);
            Assert.Null(item.RequiredNamespace);

            // <item id="css2" href="styles/big.css" media-type="text/css"/>
            item = format.Opf.Manifest.Items.First(e => e.Id == "css2");
            Assert.Equal("styles/big.css", item.Href);
            Assert.Equal("text/css", item.MediaType);
            Assert.Equal(0, item.Properties.Count);
            Assert.Null(item.Fallback);
            Assert.Null(item.FallbackStyle);
            Assert.Null(item.RequiredModules);
            Assert.Null(item.RequiredNamespace);

            /*
            <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                <dc:title>Bogtyven</dc:title>
                <dc:creator id="creator_01">Markus Zusak</dc:creator>
                <dc:publisher>Lindhardt og Ringhof</dc:publisher>
                <dc:rights>All rights reserved Lindhardt og Ringhof Forlag A/S</dc:rights>
                <dc:identifier id="ISBN9788711332412">9788711332412</dc:identifier>
                <dc:source>urn:isbn:9788711359327</dc:source>
                <dc:language>da</dc:language>
                <dc:date>2014-04-01</dc:date>
                <meta refines="#creator_01" property="role">aut</meta>
                <meta refines="#creator_01" property="file-as">Zusak, Markus</meta>
                <meta property="dcterms:modified">2014-03-19T02:42:00Z</meta>
                <meta property="ibooks:version">1.0.0</meta>
                <meta name="cover" content="cover-image"/>
                <meta property="rendition:layout">reflowable</meta>
                <meta property="ibooks:respect-image-size-class">img_ibooks</meta>
                <meta property="ibooks:specified-fonts">true</meta>
            </metadata>
             */
            Assert.NotNull(format.Opf.Metadata);
            Assert.Equal(0, format.Opf.Metadata.Contributors.Count);
            Assert.Equal(0, format.Opf.Metadata.Coverages.Count);
            Assert.Equal(1, format.Opf.Metadata.Creators.Count);
            Assert.Equal("Markus Zusak", format.Opf.Metadata.Creators.ElementAt(0).Text);
            Assert.Equal(1, format.Opf.Metadata.Dates.Count);
            Assert.Equal("2014-04-01", format.Opf.Metadata.Dates.ElementAt(0).Text);
            Assert.Equal(0, format.Opf.Metadata.Descriptions.Count);
            Assert.Equal(0, format.Opf.Metadata.Formats.Count);

            Assert.Equal(1, format.Opf.Metadata.Identifiers.Count);
            Assert.Equal("9788711332412", format.Opf.Metadata.Identifiers.ElementAt(0).Text);
            Assert.Equal("ISBN9788711332412", format.Opf.Metadata.Identifiers.ElementAt(0).Id);

            Assert.Equal(1, format.Opf.Metadata.Languages.Count);
            Assert.Equal("da", format.Opf.Metadata.Languages.ElementAt(0));

            Assert.Equal(8, format.Opf.Metadata.Metas.Count);
            Assert.True(format.Opf.Metadata.Metas.All(e => e.Id == null));
            Assert.True(format.Opf.Metadata.Metas.All(e => e.Scheme == null));

            var meta = format.Opf.Metadata.Metas.Single(e => e.Property == "dcterms:modified");
            Assert.Equal("2014-03-19T02:42:00Z", meta.Text);
            Assert.Null(meta.Id);
            Assert.Null(meta.Name);
            Assert.Null(meta.Refines);
            Assert.Null(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "ibooks:version");
            Assert.Equal("1.0.0", meta.Text);
            Assert.Null(meta.Id);
            Assert.Null(meta.Name);
            Assert.Null(meta.Refines);
            Assert.Null(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "rendition:layout");
            Assert.Equal("reflowable", meta.Text);
            Assert.Null(meta.Id);
            Assert.Null(meta.Name);
            Assert.Null(meta.Refines);
            Assert.Null(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "ibooks:respect-image-size-class");
            Assert.Equal("img_ibooks", meta.Text);
            Assert.Null(meta.Id);
            Assert.Null(meta.Name);
            Assert.Null(meta.Refines);
            Assert.Null(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "ibooks:specified-fonts");
            Assert.Equal("true", meta.Text);
            Assert.Null(meta.Id);
            Assert.Null(meta.Name);
            Assert.Null(meta.Refines);
            Assert.Null(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "role");
            Assert.Equal("aut", meta.Text);
            Assert.Equal("#creator_01", meta.Refines);
            Assert.Null(meta.Id);
            Assert.Null(meta.Name);
            Assert.Null(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "file-as");
            Assert.Equal("Zusak, Markus", meta.Text);
            Assert.Equal("#creator_01", meta.Refines);
            Assert.Null(meta.Id);
            Assert.Null(meta.Name);
            Assert.Null(meta.Scheme);

            Assert.Equal(1, format.Opf.Metadata.Publishers.Count);
            Assert.Equal("Lindhardt og Ringhof", format.Opf.Metadata.Publishers.ElementAt(0));

            Assert.Equal(0, format.Opf.Metadata.Relations.Count);

            Assert.Equal(1, format.Opf.Metadata.Rights.Count);
            Assert.Equal("All rights reserved Lindhardt og Ringhof Forlag A/S", format.Opf.Metadata.Rights.ElementAt(0));

            Assert.Equal(1, format.Opf.Metadata.Sources.Count);
            Assert.Equal("urn:isbn:9788711359327", format.Opf.Metadata.Sources.ElementAt(0));

            Assert.Equal(0, format.Opf.Metadata.Subjects.Count);
            Assert.Equal(0, format.Opf.Metadata.Types.Count);

            Assert.Equal(1, format.Opf.Metadata.Titles.Count);
            Assert.Equal("Bogtyven", format.Opf.Metadata.Titles.ElementAt(0));

            Assert.Equal(1, format.Opf.Metadata.Identifiers.Count);
            Assert.Null(format.Opf.Metadata.Identifiers.ElementAt(0).Scheme);
            Assert.Equal("ISBN9788711332412", format.Opf.Metadata.Identifiers.ElementAt(0).Id);
            Assert.Equal("9788711332412", format.Opf.Metadata.Identifiers.ElementAt(0).Text);

            Assert.Equal("ncx", format.Opf.Spine.Toc);
            Assert.Equal(108, format.Opf.Spine.ItemRefs.Count);
            Assert.Equal(6, format.Opf.Spine.ItemRefs.Count(e => e.Properties.Contains("page-spread-right")));
            Assert.Equal(1, format.Opf.Spine.ItemRefs.Count(e => e.IdRef == "body044_01"));

            Assert.Null(format.Ncx.DocAuthor);
            Assert.Equal("Bogtyven", format.Ncx.DocTitle);

            /*
            <head>
                <meta name="dtb:uid" content="9788711332412"/>
                <meta name="dtb:depth" content="1"/>
                <meta name="dtb:totalPageCount" content="568"/>
            </head>
             */
            Assert.Equal(3, format.Ncx.Meta.Count);

            Assert.Equal("dtb:uid", format.Ncx.Meta.ElementAt(0).Name);
            Assert.Equal("9788711332412", format.Ncx.Meta.ElementAt(0).Content);
            Assert.Null(format.Ncx.Meta.ElementAt(0).Scheme);

            Assert.Equal("dtb:depth", format.Ncx.Meta.ElementAt(1).Name);
            Assert.Equal("1", format.Ncx.Meta.ElementAt(1).Content);
            Assert.Null(format.Ncx.Meta.ElementAt(1).Scheme);

            Assert.Equal("dtb:totalPageCount", format.Ncx.Meta.ElementAt(2).Name);
            Assert.Equal("568", format.Ncx.Meta.ElementAt(2).Content);
            Assert.Null(format.Ncx.Meta.ElementAt(2).Scheme);

            Assert.Null(format.Ncx.NavList);
            Assert.Null(format.Ncx.PageList);

            Assert.NotNull(format.Ncx.NavMap);
            Assert.NotNull(format.Ncx.NavMap.Dom);
            Assert.Equal(111, format.Ncx.NavMap.NavPoints.Count);
            foreach (var point in format.Ncx.NavMap.NavPoints)
            {
                Assert.NotNull(point.Id);
                Assert.NotNull(point.PlayOrder);
                Assert.NotNull(point.ContentSrc);
                Assert.NotNull(point.NavLabelText);
                Assert.Null(point.Class);
                Assert.False(point.NavPoints.Any());
            }

            // <navPoint id="navPoint-38" playOrder="38"><navLabel><text>– Rosas vrede</text></navLabel><content src="chapter_032.xhtml"/></navPoint>
            var navPoint = format.Ncx.NavMap.NavPoints.Single(e => e.Id == "navPoint-38");
            Assert.Equal(38, navPoint.PlayOrder);
            Assert.Equal("– Rosas vrede", navPoint.NavLabelText);
            Assert.Equal("chapter_032.xhtml", navPoint.ContentSrc);

            Assert.Equal("Bogtyven", format.Nav.Head.Title);

            /*
                <link rel="stylesheet" href="../styles/general.css" type="text/css"/>
                <link rel="stylesheet" media="(min-width:550px) and (orientation:portrait)" href="../styles/big.css" type="text/css"/>
             */
            Assert.Equal(2, format.Nav.Head.Links.Count);

            Assert.Equal(null, format.Nav.Head.Links.ElementAt(0).Class);
            Assert.Equal(null, format.Nav.Head.Links.ElementAt(0).Title);
            Assert.Equal("../styles/general.css", format.Nav.Head.Links.ElementAt(0).Href);
            Assert.Equal("stylesheet", format.Nav.Head.Links.ElementAt(0).Rel);
            Assert.Equal("text/css", format.Nav.Head.Links.ElementAt(0).Type);
            Assert.Equal(null, format.Nav.Head.Links.ElementAt(0).Media);

            Assert.Equal(null, format.Nav.Head.Links.ElementAt(1).Class);
            Assert.Equal(null, format.Nav.Head.Links.ElementAt(1).Title);
            Assert.Equal("../styles/big.css", format.Nav.Head.Links.ElementAt(1).Href);
            Assert.Equal("stylesheet", format.Nav.Head.Links.ElementAt(1).Rel);
            Assert.Equal("text/css", format.Nav.Head.Links.ElementAt(1).Type);
            Assert.Equal("(min-width:550px) and (orientation:portrait)", format.Nav.Head.Links.ElementAt(1).Media);

            Assert.Equal(1, format.Nav.Head.Metas.Count);
            Assert.Equal("utf-8", format.Nav.Head.Metas.ElementAt(0).Charset);
            Assert.Null(format.Nav.Head.Metas.ElementAt(0).Name);
            Assert.Null(format.Nav.Head.Metas.ElementAt(0).Content);

            Assert.NotNull(format.Nav.Body);

            /*
             <nav epub:type="toc" id="toc"></nav>
             <nav epub:type="landmarks" class="hide"></nav>
             <nav epub:type="page-list" class="hide"></nav>
             */
            Assert.Equal(3, format.Nav.Body.Navs.Count);

            Assert.NotNull(format.Nav.Body.Navs.ElementAt(0).Dom);
            Assert.Equal("toc", format.Nav.Body.Navs.ElementAt(0).Type);
            Assert.Equal("toc", format.Nav.Body.Navs.ElementAt(0).Id);
            Assert.Null(format.Nav.Body.Navs.ElementAt(0).Class);
            Assert.Null(format.Nav.Body.Navs.ElementAt(0).Hidden);

            Assert.NotNull(format.Nav.Body.Navs.ElementAt(1).Dom);
            Assert.Equal("landmarks", format.Nav.Body.Navs.ElementAt(1).Type);
            Assert.Equal("hide", format.Nav.Body.Navs.ElementAt(1).Class);
            Assert.Null(format.Nav.Body.Navs.ElementAt(1).Id);
            Assert.Null(format.Nav.Body.Navs.ElementAt(1).Hidden);

            Assert.NotNull(format.Nav.Body.Navs.ElementAt(2).Dom);
            Assert.Equal("page-list", format.Nav.Body.Navs.ElementAt(2).Type);
            Assert.Equal("hide", format.Nav.Body.Navs.ElementAt(2).Class);
            Assert.Null(format.Nav.Body.Navs.ElementAt(2).Id);
            Assert.Null(format.Nav.Body.Navs.ElementAt(2).Hidden);
        }

        [Fact]
        public void ReadIOSHackersHandbookTest()
        {
            var book = EpubReader.Read(Cwd.Combine(@"Samples/epub-assorted/iOS Hackers Handbook.epub"));
            book.TableOfContents.Should().HaveCount(14);
            book.TableOfContents.SelectMany(e => e.SubChapters).Concat(book.TableOfContents).Should().HaveCount(78);
            book.TableOfContents[0].AbsolutePath.Should().Be("/OEBPS/9781118240755cover.xhtml");
            book.TableOfContents[1].AbsolutePath.Should().Be("/OEBPS/9781118240755c01.xhtml");
            book.TableOfContents[1].SubChapters.Should().HaveCount(6);
            book.TableOfContents[1].SubChapters[0].AbsolutePath.Should().Be("/OEBPS/9781118240755c01.xhtml");
        }

        [Fact]
        public void SetsChapterParents()
        {
            var book = EpubReader.Read(Cwd.Combine(@"Samples/epub-assorted/iOS Hackers Handbook.epub"));

            foreach (var chapter in book.TableOfContents)
            {
                chapter.Parent.Should().BeNull();
                chapter.SubChapters.All(e => e.Parent == chapter).Should().BeTrue();
            }
        }

        [Fact]
        public void SetsChapterPreviousNext()
        {
            var book = EpubReader.Read(Cwd.Combine(@"Samples/epub-assorted/iOS Hackers Handbook.epub"));

            EpubChapter previousChapter = null;
            var currentChapter = book.TableOfContents[0];
            currentChapter.Previous.Should().Be(previousChapter);

            for (var i = 1; i <= 77; ++i)
            {
                previousChapter = currentChapter;
                currentChapter = currentChapter.Next;

                previousChapter.Next.Should().Be(currentChapter);
                currentChapter.Previous.Should().Be(previousChapter);
            }

            EpubChapter nextChapter = null;
            currentChapter.Next.Should().Be(nextChapter);
            
            for (var i = 1; i <= 77; ++i)
            {
                nextChapter = currentChapter;
                currentChapter = currentChapter.Previous;

                currentChapter.Next.Should().Be(nextChapter);
                nextChapter.Previous.Should().Be(currentChapter);
            }

            currentChapter.Previous.Should().BeNull();
        }
    }
}
