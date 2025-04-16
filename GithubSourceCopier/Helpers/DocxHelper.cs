using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace GithubSourceCopier.Helpers;

public static class DocxHelper
{
    public static void CreateDocx(Stream stream, IEnumerable<(string Type, string Text)> elements)
    {
        using var word = WordprocessingDocument.Create(stream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true);
        var main = word.AddMainDocumentPart();
        main.Document = new Document(new Body());

        foreach (var el in elements)
        {
            Paragraph p = new Paragraph();
            Run r = new Run();
            switch (el.Type)
            {
                case "h1":
                    r.Append(new Text(el.Text) { Space = SpaceProcessingModeValues.Preserve });
                    p.Append(r);
                    p.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = "Heading1" });
                    break;
                case "h2":
                    r.Append(new Text(el.Text) { Space = SpaceProcessingModeValues.Preserve });
                    p.Append(r);
                    p.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = "Heading2" });
                    break;
                case "p":
                    r.Append(new Text(el.Text) { Space = SpaceProcessingModeValues.Preserve });
                    p.Append(r);
                    break;
                case "code":
                    r.Append(new Text(el.Text) { Space = SpaceProcessingModeValues.Preserve });
                    p.Append(r);
                    p.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = "Code" });
                    break;
            }
            main.Document.Body.Append(p);
        }

        main.Document.Save();
    }
}
