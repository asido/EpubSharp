# EpubSharp
C# library for reading and writing EPUB files

# Usage

```cs
// Read an epub file
EpubBook book = EpubReader.Read("my.epub");

// Convert to plain text.
string text = book.ToPlainText();
```
