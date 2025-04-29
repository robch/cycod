using System;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;

class HtmlHelpers
{
    public static string StripHtmlContent(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var innerText = WebUtility.HtmlDecode(doc.DocumentNode.InnerText);
        var innerTextLines = innerText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        var lines = new List<string>();
        var blanks = new List<string>();
        foreach (var line in innerTextLines)
        {
            var trimmed = line.Trim(new[] { ' ', '\t', '\r', '\n', '\v', '\f', '\u00A0', '\u200B' });
            if (string.IsNullOrEmpty(trimmed))
            {
                blanks.Add(line);
                continue;
            }

            // Only insert one blank line at a time
            var addBlanks = Math.Min(blanks.Count, 1);
            while (addBlanks-- > 0) lines.Add(string.Empty);
            blanks.Clear();

            lines.Add(line);
        }
        return string.Join(Environment.NewLine, lines);
    }
}