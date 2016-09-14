# EpubSharp
C# library for reading and writing EPUB files.

Supported EPUB versions: **2.0**, **3.0**, **3.1**

# Usage

### Reading an EPUB

```cs
// Read an epub file
EpubBook book = EpubReader.Read("my.epub");

// Read metadata
string title = book.Title;
string[] authors = book.Authors;
Image cover = book.CoverImage;

// Get table of contents
IReadOnlyCollection<EpubChapter> chapters = book.TableOfContents;

// Get contained files
IReadOnlyCollection<EpubTextFile> html = book.Resources.Html;
IReadOnlyCollection<EpubTextFile> css = book.Resources.Css;
IReadOnlyCollection<EpubByteFile> images = book.Resources.Images;
IReadOnlyCollection<EpubByteFile> fonts = book.Resources.Fonts;

// Convert to plain text
string text = book.ToPlainText();

// Access internal EPUB format specific data structures.
EpubFormat format = book.Format;
OcfDocument ocf = format.Ocf;
OpfDocument opf = format.Opf;
NcxDocument ncx = format.Ncx;
NavDocument nav = format.Nav;

// Create an EPUB
EpubWriter.Write(book, "new.epub");
```

### Writing an EPUB
_**Ability to create an epub is comming in the near future**_
