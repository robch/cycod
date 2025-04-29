using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

public class PdfFileConverter : IFileConverter
{
    public bool CanConvert(string fileName)
    {
        return fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public string ConvertToMarkdown(string fileName)
    {
        var sb = new StringBuilder();
        using (var document = PdfDocument.Open(fileName))
        {
            int pageCount = document.NumberOfPages;
            for (int pageIndex = 1; pageIndex <= pageCount; pageIndex++)
            {
                var page = document.GetPage(pageIndex);

                sb.AppendLine($"## Page {pageIndex}");
                sb.AppendLine();

                var pageMarkdown = ConvertPageToMarkdown(page);
                sb.AppendLine(pageMarkdown);
            }
        }

        return sb.ToString();
    }

    private string ConvertPageToMarkdown(Page page)
    {
        var words = page.GetWords(NearestNeighbourWordExtractor.Instance);
        var blocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);

        var unsupervisedReadingOrderDetector = new UnsupervisedReadingOrderDetector(10);
        var orderedBlocks = unsupervisedReadingOrderDetector.Get(blocks);

        var sb = new StringBuilder();
        foreach (var block in orderedBlocks)
        {
            var blockWords = block.Text;
            sb.AppendLine(blockWords);
            sb.AppendLine();
        }

        sb.AppendLine();

        return sb.ToString();
    }
}
