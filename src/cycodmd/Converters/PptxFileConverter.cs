using System;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

public class PptxFileConverter : IFileConverter
{
    private enum ListType { None, Bullet, Numbered }

    public bool CanConvert(string fileName)
    {
        return fileName.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase);
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
        using var presentationDoc = PresentationDocument.Open(fileName, false);
        var sb = new StringBuilder();

        var presentationPart = presentationDoc.PresentationPart;
        if (presentationPart == null)
        {
            return string.Empty;
        }

        var slideNumber = 1;
        var slideIdList = presentationPart.Presentation.SlideIdList;
        foreach (var slideId in slideIdList.Elements<SlideId>())
        {
            var slidePart = presentationPart.GetPartById(slideId.RelationshipId) as SlidePart;
            sb.AppendLine(ConvertSlideToMarkdown(slidePart, slideNumber++));
        }

        return sb.ToString();
    }

    private string ConvertSlideToMarkdown(SlidePart slidePart, int slideNumber)
    {
        var sb = new StringBuilder();
        var slide = slidePart.Slide;

        // Extract the slide title
        var titleShape = slide.Descendants<Shape>().FirstOrDefault(s => IsTitleShape(s));
        if (titleShape != null)
        {
            var titleText = GetTextFromShape(slidePart, titleShape, ": ");
            sb.AppendLine($"# Slide #{slideNumber}: {titleText}\n");
        }
        else
        {
            sb.AppendLine($"# Slide #{slideNumber}\n");
        }

        // Extract other text boxes and shapes
        foreach (var shape in slide.Descendants<Shape>().Where(s => !IsTitleShape(s)))
        {
            var text = GetTextFromShape(slidePart, shape);
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text);
            }
        }

        // Extract tables
        foreach (var graphicFrame in slide.Descendants<GraphicFrame>())
        {
            var tableMarkdown = ConvertTableToMarkdown(slidePart, graphicFrame);
            if (!string.IsNullOrWhiteSpace(tableMarkdown))
            {
                sb.AppendLine();
                sb.AppendLine(tableMarkdown);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private bool IsTitleShape(Shape shape)
    {
        var placeholder = shape.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.PlaceholderShape;
        return placeholder != null && placeholder.Type != null &&
            placeholder.Type.HasValue && placeholder.Type.Value == PlaceholderValues.Title;
    }

    private string GetTextFromShape(SlidePart slidePart, Shape shape, string insertForBreak = "\n")
    {
        if (shape.TextBody == null)
            return string.Empty;

        var sb = new StringBuilder();
        
        bool firstParagraph = true;
        var paragraphs = shape.TextBody.Elements<A.Paragraph>();
        foreach (var paragraph in paragraphs)
        {
            var paragraphText = GetParagraphText(slidePart, paragraph, insertForBreak);
            if (string.IsNullOrEmpty(paragraphText)) continue;

            if (!firstParagraph)
            {
                sb.AppendLine();
            }
            sb.Append(paragraphText);
            firstParagraph = false;
        }

        return sb.ToString();
    }

    private string GetParagraphText(SlidePart slidePart, A.Paragraph paragraph, string insertForBreak)
    {
        var paragraphText = new StringBuilder();
        var listType = GetListType(paragraph);
        var paragraphPrefix = listType switch
        {
            ListType.Bullet => "- ",
            ListType.Numbered => "1. ",
            _ => string.Empty
        };

        bool boldOn = false;
        bool italicOn = false;
        bool insertBreakNextTime = false;

        foreach (var element in paragraph.Elements())
        {
            if (element is A.Run run)
            {
                if (insertBreakNextTime)
                {
                    paragraphText.Append(insertForBreak);
                    insertBreakNextTime = false;
                }
                HandleParagraphRun(paragraphText, ref boldOn, ref italicOn, run, slidePart);
            }
            else if (element is A.Break)
            {
                insertBreakNextTime = true;
            }
        }

        CheckTurnOffBoldItalic(paragraphText, ref boldOn, ref italicOn);

        var trimmed = paragraphText.ToString().TrimEnd();
        return !string.IsNullOrEmpty(trimmed)
            ? $"{paragraphPrefix}{trimmed}"
            : string.Empty;
    }

    private static void HandleParagraphRun(StringBuilder paragraphText, ref bool boldOn, ref bool italicOn, A.Run run, SlidePart slidePart)
    {
        var runText = run.Text?.Text ?? string.Empty;
        if (string.IsNullOrEmpty(runText)) return;

        var runProps = run.RunProperties;
        var isBold = runProps?.Bold?.Value == true;
        var isItalic = runProps?.Italic?.Value == true;

        var turnBoldOn = isBold && !boldOn;
        var turnBoldOff = !isBold && boldOn;
        var turnItalicOn = isItalic && !italicOn;
        var turnItalicOff = !isItalic && italicOn;

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

        var hyperlink = run.Descendants<A.HyperlinkOnClick>().FirstOrDefault();
        if (hyperlink != null)
        {
            runText = ConvertHyperlinkToMarkdown(slidePart, hyperlink, runText);
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

    private ListType GetListType(A.Paragraph paragraph)
    {
        var paragraphProperties = paragraph.ParagraphProperties;
        if (paragraphProperties != null)
        {
            var bullet = paragraphProperties.Descendants<A.CharacterBullet>().FirstOrDefault();
            if (bullet != null)
            {
                return ListType.Bullet;
            }

            var autoNumberedBullet = paragraphProperties.Descendants<A.AutoNumberedBullet>().FirstOrDefault();
            if (autoNumberedBullet != null)
            {
                return ListType.Numbered;
            }
        }

        return ListType.None;
    }

    private string ConvertTableToMarkdown(SlidePart slidePart, GraphicFrame graphicFrame)
    {
        var table = graphicFrame.Descendants<A.Table>().FirstOrDefault();
        if (table == null) return string.Empty;

        var sb = new StringBuilder();
        foreach (var row in table.Elements<A.TableRow>())
        {
            var rowTexts = row.Elements<A.TableCell>()
                .Select(cell => GetCellText(slidePart, cell))
                .ToArray();
            sb.AppendLine("| " + string.Join(" | ", rowTexts) + " |");

            if (row == table.Elements<A.TableRow>().First())
                sb.AppendLine("|" + string.Join("|", rowTexts.Select(_ => "---")) + "|");
        }

        return sb.ToString();
    }

    private string GetCellText(SlidePart slidePart, A.TableCell cell)
    {
        var cellText = new StringBuilder();
        var isFirst = true;
        foreach (var paragraph in cell.TextBody.Elements<A.Paragraph>())
        {
            var paragraphText = GetParagraphText(slidePart, paragraph, "<br/>").TrimEnd();
            if (string.IsNullOrWhiteSpace(paragraphText)) continue;

            if (!isFirst)
                cellText.Append("<br/>");
            else
                isFirst = false;

            cellText.Append(paragraphText);
        }

        return cellText.ToString().Trim();
    }

    private static string ConvertHyperlinkToMarkdown(SlidePart slidePart, A.HyperlinkOnClick hyperlink, string displayText)
    {
        var hyperlinkRelationship = slidePart.HyperlinkRelationships.FirstOrDefault(h => h.Id == hyperlink.Id);
        if (hyperlinkRelationship == null) return string.Empty;

        var uri = hyperlinkRelationship.Uri.OriginalString;
        return $"[{displayText}]({uri})";
    }
}
