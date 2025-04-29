using System;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

public class DocxFileConverter : IFileConverter
{
    private enum ListType { None, Bullet, Numbered }

    public bool CanConvert(string fileName)
    {
        return fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase);
    }

    public string ConvertToMarkdown(string fileName)
    {
        try
        {
            return TryConvertToMarkdown(fileName);
        }
        catch (Exception ex)
        {
            var couldBeEncrypted = ex.Message.Contains("corrupted");
            if (couldBeEncrypted) return $"File encrypted or corrupted: {fileName}\n\nPlease remove encryption or fix the file and try again.";
            throw;
        }
    }

    private string TryConvertToMarkdown(string fileName)
    {
        using var doc = WordprocessingDocument.Open(fileName, false);
        var sb = new StringBuilder();

        var mainPart = doc.MainDocumentPart;
        if (mainPart?.Document?.Body == null)
        {
            return string.Empty;
        }

        foreach (var block in mainPart.Document.Body.Elements())
        {
            if (block is Paragraph paragraph)
            {
                var paragraphText = GetParagraphText(paragraph, doc);
                sb.AppendLine(paragraphText);
            }
            else if (block is Table table)
            {
                sb.AppendLine(ConvertTableToMarkdown(table, doc));
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private string GetParagraphText(Paragraph paragraph, WordprocessingDocument doc)
    {
        var listType = GetListType(paragraph, doc);
        var paragraphSuffix = listType == ListType.None ? Environment.NewLine : string.Empty;
        var paragraphPrefix = listType switch
        {
            ListType.Bullet => "- ",
            ListType.Numbered => "1. ",
            _ => string.Empty
        };

        var style = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
        var headingLevel = GetHeadingLevel(style, doc);
        if (headingLevel > 0)
        {
            var hashMarks = new string('#', headingLevel);
            paragraphPrefix = $"{hashMarks} {paragraphPrefix}";
        }

        var paragraphText = new StringBuilder();
        paragraphText.Append(paragraphPrefix);

        var boldOn = false;
        var italicOn = false;

        foreach (var element in paragraph.Elements())
        {
            if (element is Run run)
            {
                HandleParagraphRun(paragraphText, ref boldOn, ref italicOn, run);
            }
            else if (element is Hyperlink hyperlink)
            {
                var hyperlinkMarkdown = ConvertHyperlinkToMarkdown(hyperlink, doc);
                paragraphText.Append(hyperlinkMarkdown);
            }
        }

        CheckTurnOffBoldItalic(paragraphText, ref boldOn, ref italicOn);

        paragraphText.Append(paragraphSuffix);

        return paragraphText.ToString();
    }

    private static void HandleParagraphRun(StringBuilder paragraphText, ref bool boldOn, ref bool italicOn, Run run)
    {
        string runText = string.Join("", run.Elements<Text>().Select(t => t.Text));
        if (string.IsNullOrEmpty(runText)) return;

        var bold = run.RunProperties?.Bold != null;
        var italic = run.RunProperties?.Italic != null;

        var turnBoldOn = bold && !boldOn;
        var turnBoldOff = !bold && boldOn;
        var turnItalicOn = italic && !italicOn;
        var turnItalicOff = !italic && italicOn;

        var turnAnythingOff = turnBoldOff || turnItalicOff;
        var turnAnythingOn = turnBoldOn || turnItalicOn;

        if (turnAnythingOff)
        {
            var cchTrailingWhitespace = paragraphText.Length - paragraphText.ToString().TrimEnd().Length;
            var trailingWhitespace = paragraphText.ToString().Substring(paragraphText.Length - cchTrailingWhitespace);
            paragraphText.Length -= cchTrailingWhitespace;

            if (turnItalicOff) { paragraphText.Append("_"); italicOn = false; }
            if (turnBoldOff) { paragraphText.Append("**"); boldOn = false; }

            paragraphText.Append(trailingWhitespace);
        }

        if (turnAnythingOn)
        {
            var cchLeadingWhitespace = runText.Length - runText.TrimStart().Length;
            var leadingWhitespace = runText.Substring(0, cchLeadingWhitespace);
            runText = runText.TrimStart();

            paragraphText.Append(leadingWhitespace);

            if (turnBoldOn) { paragraphText.Append("**"); boldOn = true; }
            if (turnItalicOn) { paragraphText.Append("_"); italicOn = true; }
        }

        paragraphText.Append(runText);
    }

    private static void CheckTurnOffBoldItalic(StringBuilder paragraphText, ref bool boldOn, ref bool italicOn)
    {
        var finalTurnOffBold = boldOn;
        var finalTurnOffItalic = italicOn;
        var finalTurnAnythingOff = finalTurnOffBold || finalTurnOffItalic;

        if (finalTurnAnythingOff)
        {
            var cchFinalTrailingWhitespace = paragraphText.ToString().Length - paragraphText.ToString().TrimEnd().Length;
            var finalTrailingWhitespace = paragraphText.ToString().Substring(paragraphText.Length - cchFinalTrailingWhitespace);
            paragraphText.Length -= cchFinalTrailingWhitespace;

            if (finalTurnOffItalic) { paragraphText.Append("_"); italicOn = false; }
            if (finalTurnOffBold) { paragraphText.Append("**"); boldOn = false; }

            paragraphText.Append(finalTrailingWhitespace);
        }
    }

    private int GetHeadingLevel(string styleId, WordprocessingDocument doc)
    {
        if (string.IsNullOrEmpty(styleId)) return 0;

        // OpenXML can define a style named "Heading1", "Heading2", etc. 
        if (styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase))
        {
            var levelStr = styleId.Substring("Heading".Length);
            if (int.TryParse(levelStr, out int level))
            {
                return Math.Min(level, 6); // Markdown supports up to 6 levels
            }
        }

        return 0;
    }

    private ListType GetListType(Paragraph para, WordprocessingDocument doc)
    {
        var numberingId = para.ParagraphProperties?.NumberingProperties?.NumberingId?.Val;
        if (numberingId is null) return ListType.None;

        var numberingPart = doc.MainDocumentPart.NumberingDefinitionsPart;
        if (numberingPart == null) return ListType.None;

        var abstractNumId = numberingPart.Numbering
             .Elements<NumberingInstance>()
             .First(i => i.NumberID.Value == numberingId.Value)
             ?.AbstractNumId.Val;

        var abstractNum = numberingPart.Numbering
            .Elements<AbstractNum>()
            .FirstOrDefault(an => an.AbstractNumberId?.Value == abstractNumId.Value);

        var level = abstractNum?.Elements<Level>()
            .FirstOrDefault(lvl => lvl.LevelIndex?.Value == 0);

        var numFmt = level?.NumberingFormat?.Val;

        if (numFmt != null && numFmt.Value == NumberFormatValues.Bullet)
            return ListType.Bullet;
        else
            return ListType.Numbered;
    }

    private string ConvertTableToMarkdown(Table table, WordprocessingDocument doc)
    {
        var sb = new StringBuilder();

        foreach (var row in table.Elements<TableRow>())
        {
            var rowTexts = row.Elements<TableCell>()
                .Select(cell => GetCellText(cell, doc))
                .ToArray();
            sb.AppendLine("| " + string.Join(" | ", rowTexts) + " |");

            if (row == table.Elements<TableRow>().First())
                sb.AppendLine("|" + string.Join("|", rowTexts.Select(_ => "---")) + "|");
        }

        return sb.ToString();
    }

    private string GetCellText(TableCell cell, WordprocessingDocument doc)
    {
        var cellText = new StringBuilder();
        var isFirst = true;
        foreach (var paragraph in cell.Elements<Paragraph>())
        {
            var paragraphText = GetParagraphText(paragraph, doc).TrimEnd();
            if (string.IsNullOrWhiteSpace(paragraphText)) continue;

            if (!isFirst)
                cellText.Append("<br/>");
            else
                isFirst = false;

            cellText.Append(paragraphText);
        }

        return cellText.ToString().Trim();
    }

    private string ConvertHyperlinkToMarkdown(Hyperlink hyperlink, WordprocessingDocument doc)
    {
        var relationshipId = hyperlink.Id?.Value;
        if (relationshipId == null) return string.Empty;

        var hyperlinkPart = doc.MainDocumentPart.HyperlinkRelationships.FirstOrDefault(r => r.Id == relationshipId);
        if (hyperlinkPart == null) return string.Empty;

        var uri = hyperlinkPart.Uri.OriginalString;
        var displayText = hyperlink.InnerText;

        return $"[{displayText}]({uri})";
    }
}