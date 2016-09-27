using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpubSharp.Format;
using NUnit.Framework;

namespace EpubSharp.Tests
{
    [TestFixture]
    public class EpubReaderTests
    {
        [Test]
        public void ReadBogtyvenFormatTest()
        {
            var book = EpubReader.Read(@"Samples/epub-assorted/Bogtyven.epub");
            var format = book.Format;

            Assert.IsNotNull(format, nameof(format));

            Assert.IsNotNull(format.Ocf, nameof(format.Ocf));
            Assert.AreEqual(1, format.Ocf.RootFiles.Count);
            Assert.AreEqual("OPS/9788711332412.opf", format.Ocf.RootFiles.ElementAt(0).FullPath);
            Assert.AreEqual("application/oebps-package+xml", format.Ocf.RootFiles.ElementAt(0).MediaType);
            Assert.AreEqual("OPS/9788711332412.opf", format.Ocf.RootFilePath, nameof(format.Ocf.RootFilePath));

            Assert.IsNotNull(format.Opf, nameof(format.Opf));
            Assert.AreEqual(EpubVersion.Epub3, format.Opf.EpubVersion);

            /*
            <guide>
                <reference type="cover" href="xhtml/cover.xhtml"/>
                <reference type="title-page" href="xhtml/title.xhtml"/>
                <reference type="chapter" href="xhtml/prologue.xhtml"/>
                <reference type="copyright-page" href="xhtml/copyright.xhtml"/>
            </guide>
             */
            Assert.IsNotNull(format.Opf.Guide, nameof(format.Opf.Guide));
            Assert.AreEqual(4, format.Opf.Guide.References.Count);

            Assert.AreEqual("xhtml/cover.xhtml", format.Opf.Guide.References.ElementAt(0).Href);
            Assert.AreEqual(null, format.Opf.Guide.References.ElementAt(0).Title);
            Assert.AreEqual("cover", format.Opf.Guide.References.ElementAt(0).Type);

            Assert.AreEqual("xhtml/title.xhtml", format.Opf.Guide.References.ElementAt(1).Href);
            Assert.AreEqual(null, format.Opf.Guide.References.ElementAt(1).Title);
            Assert.AreEqual("title-page", format.Opf.Guide.References.ElementAt(1).Type);

            Assert.AreEqual("xhtml/prologue.xhtml", format.Opf.Guide.References.ElementAt(2).Href);
            Assert.AreEqual(null, format.Opf.Guide.References.ElementAt(2).Title);
            Assert.AreEqual("chapter", format.Opf.Guide.References.ElementAt(2).Type);

            Assert.AreEqual("xhtml/copyright.xhtml", format.Opf.Guide.References.ElementAt(3).Href);
            Assert.AreEqual(null, format.Opf.Guide.References.ElementAt(3).Title);
            Assert.AreEqual("copyright-page", format.Opf.Guide.References.ElementAt(3).Type);

            Assert.IsNotNull(format.Opf.Manifest, nameof(format.Opf.Manifest));
            Assert.AreEqual(150, format.Opf.Manifest.Items.Count);

            // <item id="body097" href="xhtml/chapter_083.xhtml" media-type="application/xhtml+xml" properties="svg"/>
            var item = format.Opf.Manifest.Items.First(e => e.Id == "body097");
            Assert.AreEqual("xhtml/chapter_083.xhtml", item.Href);
            Assert.AreEqual("application/xhtml+xml", item.MediaType);
            Assert.AreEqual(1, item.Properties.Count);
            Assert.AreEqual("svg", item.Properties.ElementAt(0));
            Assert.IsNull(item.Fallback);
            Assert.IsNull(item.FallbackStyle);
            Assert.IsNull(item.RequiredModules);
            Assert.IsNull(item.RequiredNamespace);

            // <item id="css2" href="styles/big.css" media-type="text/css"/>
            item = format.Opf.Manifest.Items.First(e => e.Id == "css2");
            Assert.AreEqual("styles/big.css", item.Href);
            Assert.AreEqual("text/css", item.MediaType);
            Assert.AreEqual(0, item.Properties.Count);
            Assert.IsNull(item.Fallback);
            Assert.IsNull(item.FallbackStyle);
            Assert.IsNull(item.RequiredModules);
            Assert.IsNull(item.RequiredNamespace);

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
            Assert.IsNotNull(format.Opf.Metadata);
            Assert.AreEqual(0, format.Opf.Metadata.Contributors.Count);
            Assert.AreEqual(0, format.Opf.Metadata.Coverages.Count);
            Assert.AreEqual(1, format.Opf.Metadata.Creators.Count);
            Assert.AreEqual("Markus Zusak", format.Opf.Metadata.Creators.ElementAt(0).Text);
            Assert.AreEqual(1, format.Opf.Metadata.Dates.Count);
            Assert.AreEqual("2014-04-01", format.Opf.Metadata.Dates.ElementAt(0).Text);
            Assert.AreEqual(0, format.Opf.Metadata.Descriptions.Count);
            Assert.AreEqual(0, format.Opf.Metadata.Formats.Count);

            Assert.AreEqual(1, format.Opf.Metadata.Identifiers.Count);
            Assert.AreEqual("9788711332412", format.Opf.Metadata.Identifiers.ElementAt(0).Text);
            Assert.AreEqual("ISBN9788711332412", format.Opf.Metadata.Identifiers.ElementAt(0).Id);

            Assert.AreEqual(1, format.Opf.Metadata.Languages.Count);
            Assert.AreEqual("da", format.Opf.Metadata.Languages.ElementAt(0));

            Assert.AreEqual(8, format.Opf.Metadata.Metas.Count);
            Assert.IsTrue(format.Opf.Metadata.Metas.All(e => e.Id == null));
            Assert.IsTrue(format.Opf.Metadata.Metas.All(e => e.Scheme == null));

            var meta = format.Opf.Metadata.Metas.Single(e => e.Property == "dcterms:modified");
            Assert.AreEqual("2014-03-19T02:42:00Z", meta.Text);
            Assert.IsNull(meta.Id);
            Assert.IsNull(meta.Name);
            Assert.IsNull(meta.Refines);
            Assert.IsNull(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "ibooks:version");
            Assert.AreEqual("1.0.0", meta.Text);
            Assert.IsNull(meta.Id);
            Assert.IsNull(meta.Name);
            Assert.IsNull(meta.Refines);
            Assert.IsNull(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "rendition:layout");
            Assert.AreEqual("reflowable", meta.Text);
            Assert.IsNull(meta.Id);
            Assert.IsNull(meta.Name);
            Assert.IsNull(meta.Refines);
            Assert.IsNull(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "ibooks:respect-image-size-class");
            Assert.AreEqual("img_ibooks", meta.Text);
            Assert.IsNull(meta.Id);
            Assert.IsNull(meta.Name);
            Assert.IsNull(meta.Refines);
            Assert.IsNull(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "ibooks:specified-fonts");
            Assert.AreEqual("true", meta.Text);
            Assert.IsNull(meta.Id);
            Assert.IsNull(meta.Name);
            Assert.IsNull(meta.Refines);
            Assert.IsNull(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "role");
            Assert.AreEqual("aut", meta.Text);
            Assert.AreEqual("#creator_01", meta.Refines);
            Assert.IsNull(meta.Id);
            Assert.IsNull(meta.Name);
            Assert.IsNull(meta.Scheme);

            meta = format.Opf.Metadata.Metas.Single(e => e.Property == "file-as");
            Assert.AreEqual("Zusak, Markus", meta.Text);
            Assert.AreEqual("#creator_01", meta.Refines);
            Assert.IsNull(meta.Id);
            Assert.IsNull(meta.Name);
            Assert.IsNull(meta.Scheme);

            Assert.AreEqual(1, format.Opf.Metadata.Publishers.Count);
            Assert.AreEqual("Lindhardt og Ringhof", format.Opf.Metadata.Publishers.ElementAt(0));

            Assert.AreEqual(0, format.Opf.Metadata.Relations.Count);

            Assert.AreEqual(1, format.Opf.Metadata.Rights.Count);
            Assert.AreEqual("All rights reserved Lindhardt og Ringhof Forlag A/S", format.Opf.Metadata.Rights.ElementAt(0));

            Assert.AreEqual(1, format.Opf.Metadata.Sources.Count);
            Assert.AreEqual("urn:isbn:9788711359327", format.Opf.Metadata.Sources.ElementAt(0));

            Assert.AreEqual(0, format.Opf.Metadata.Subjects.Count);
            Assert.AreEqual(0, format.Opf.Metadata.Types.Count);

            Assert.AreEqual(1, format.Opf.Metadata.Titles.Count);
            Assert.AreEqual("Bogtyven", format.Opf.Metadata.Titles.ElementAt(0));

            Assert.AreEqual("ncx", format.Opf.Spine.Toc);
            Assert.AreEqual(108, format.Opf.Spine.ItemRefs.Count);
            Assert.AreEqual(6, format.Opf.Spine.ItemRefs.Count(e => e.Properties.Contains("page-spread-right")));
            Assert.AreEqual(1, format.Opf.Spine.ItemRefs.Count(e => e.IdRef == "body044_01"));

            Assert.IsNull(format.Ncx.DocAuthor);
            Assert.AreEqual("Bogtyven", format.Ncx.DocTitle);

            /*
            <head>
                <meta name="dtb:uid" content="9788711332412"/>
                <meta name="dtb:depth" content="1"/>
                <meta name="dtb:totalPageCount" content="568"/>
            </head>
             */
            Assert.AreEqual(3, format.Ncx.Meta.Count);

            Assert.AreEqual("dtb:uid", format.Ncx.Meta.ElementAt(0).Name);
            Assert.AreEqual("9788711332412", format.Ncx.Meta.ElementAt(0).Content);
            Assert.IsNull(format.Ncx.Meta.ElementAt(0).Scheme);

            Assert.AreEqual("dtb:depth", format.Ncx.Meta.ElementAt(1).Name);
            Assert.AreEqual("1", format.Ncx.Meta.ElementAt(1).Content);
            Assert.IsNull(format.Ncx.Meta.ElementAt(1).Scheme);

            Assert.AreEqual("dtb:totalPageCount", format.Ncx.Meta.ElementAt(2).Name);
            Assert.AreEqual("568", format.Ncx.Meta.ElementAt(2).Content);
            Assert.IsNull(format.Ncx.Meta.ElementAt(2).Scheme);

            Assert.IsNull(format.Ncx.NavList);
            Assert.IsNull(format.Ncx.PageList);

            Assert.IsNotNull(format.Ncx.NavMap);
            Assert.IsNotNull(format.Ncx.NavMap.Dom);
            Assert.AreEqual(111, format.Ncx.NavMap.NavPoints.Count);
            foreach (var point in format.Ncx.NavMap.NavPoints)
            {
                Assert.IsNotNull(point.Id);
                Assert.IsNotNull(point.PlayOrder);
                Assert.IsNotNull(point.ContentSrc);
                Assert.IsNotNull(point.NavLabelText);
                Assert.IsNull(point.Class);
                Assert.IsFalse(point.NavPoints.Any());
            }

            // <navPoint id="navPoint-38" playOrder="38"><navLabel><text>– Rosas vrede</text></navLabel><content src="chapter_032.xhtml"/></navPoint>
            var navPoint = format.Ncx.NavMap.NavPoints.Single(e => e.Id == "navPoint-38");
            Assert.AreEqual(38, navPoint.PlayOrder);
            Assert.AreEqual("– Rosas vrede", navPoint.NavLabelText);
            Assert.AreEqual("chapter_032.xhtml", navPoint.ContentSrc);

            Assert.AreEqual("Bogtyven", format.Nav.Head.Title);

            /*
                <link rel="stylesheet" href="../styles/general.css" type="text/css"/>
                <link rel="stylesheet" media="(min-width:550px) and (orientation:portrait)" href="../styles/big.css" type="text/css"/>
             */
            Assert.AreEqual(2, format.Nav.Head.Links.Count);

            Assert.AreEqual(null, format.Nav.Head.Links.ElementAt(0).Class);
            Assert.AreEqual(null, format.Nav.Head.Links.ElementAt(0).Title);
            Assert.AreEqual("../styles/general.css", format.Nav.Head.Links.ElementAt(0).Href);
            Assert.AreEqual("stylesheet", format.Nav.Head.Links.ElementAt(0).Rel);
            Assert.AreEqual("text/css", format.Nav.Head.Links.ElementAt(0).Type);
            Assert.AreEqual(null, format.Nav.Head.Links.ElementAt(0).Media);

            Assert.AreEqual(null, format.Nav.Head.Links.ElementAt(1).Class);
            Assert.AreEqual(null, format.Nav.Head.Links.ElementAt(1).Title);
            Assert.AreEqual("../styles/big.css", format.Nav.Head.Links.ElementAt(1).Href);
            Assert.AreEqual("stylesheet", format.Nav.Head.Links.ElementAt(1).Rel);
            Assert.AreEqual("text/css", format.Nav.Head.Links.ElementAt(1).Type);
            Assert.AreEqual("(min-width:550px) and (orientation:portrait)", format.Nav.Head.Links.ElementAt(1).Media);

            Assert.AreEqual(1, format.Nav.Head.Metas.Count);
            Assert.AreEqual("utf-8", format.Nav.Head.Metas.ElementAt(0).Charset);
            Assert.IsNull(format.Nav.Head.Metas.ElementAt(0).Name);
            Assert.IsNull(format.Nav.Head.Metas.ElementAt(0).Content);

            Assert.IsNotNull(format.Nav.Body);

            /*
             <nav epub:type="toc" id="toc"></nav>
             <nav epub:type="landmarks" class="hide"></nav>
             <nav epub:type="page-list" class="hide"></nav>
             */
            Assert.AreEqual(3, format.Nav.Body.Navs.Count);

            Assert.IsNotNull(format.Nav.Body.Navs.ElementAt(0).Dom);
            Assert.AreEqual("toc", format.Nav.Body.Navs.ElementAt(0).Type);
            Assert.AreEqual("toc", format.Nav.Body.Navs.ElementAt(0).Id);
            Assert.IsNull(format.Nav.Body.Navs.ElementAt(0).Class);
            Assert.IsNull(format.Nav.Body.Navs.ElementAt(0).Hidden);

            Assert.IsNotNull(format.Nav.Body.Navs.ElementAt(1).Dom);
            Assert.AreEqual("landmarks", format.Nav.Body.Navs.ElementAt(1).Type);
            Assert.AreEqual("hide", format.Nav.Body.Navs.ElementAt(1).Class);
            Assert.IsNull(format.Nav.Body.Navs.ElementAt(1).Id);
            Assert.IsNull(format.Nav.Body.Navs.ElementAt(1).Hidden);

            Assert.IsNotNull(format.Nav.Body.Navs.ElementAt(2).Dom);
            Assert.AreEqual("page-list", format.Nav.Body.Navs.ElementAt(2).Type);
            Assert.AreEqual("hide", format.Nav.Body.Navs.ElementAt(2).Class);
            Assert.IsNull(format.Nav.Body.Navs.ElementAt(2).Id);
            Assert.IsNull(format.Nav.Body.Navs.ElementAt(2).Hidden);
        }
    }
}
