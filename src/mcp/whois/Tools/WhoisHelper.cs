using System.Text.RegularExpressions;

namespace CycodMcp.Whois.Tools
{
    /// <summary>
    /// Helper class for processing WHOIS responses.
    /// </summary>
    internal static class WhoisHelper
    {
        /// <summary>
        /// Extracts key information from a raw WHOIS response.
        /// </summary>
        public static Dictionary<string, string> ExtractKeyInfo(string rawResponse)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                return result;
            }

            // Common patterns to extract from WHOIS data
            var patterns = new Dictionary<string, string>
            {
                { "Domain Name", @"[Dd]omain\s+[Nn]ame:\s*(.+)" },
                { "Registrar", @"[Rr]egistrar:\s*(.+)" },
                { "Creation Date", @"[Cc]reation\s+[Dd]ate:\s*(.+)" },
                { "Updated Date", @"[Uu]pdated?\s+[Dd]ate:\s*(.+)" },
                { "Expiration Date", @"(?:[Ee]xpir(?:y|ation)|[Rr]egistry\s+[Ee]xpiry)\s+[Dd]ate:\s*(.+)" },
                { "Status", @"[Ss]tatus:\s*(.+)" },
                { "Name Servers", @"[Nn]ame\s+[Ss]erver(?:s)?:\s*(.+)" },
                { "DNSSEC", @"[Dd][Nn][Ss][Ss][Ee][Cc]:\s*(.+)" }
            };

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(rawResponse, pattern.Value);
                if (matches.Count > 0)
                {
                    var values = matches
                        .Select(m => m.Groups[1].Value.Trim())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .Distinct();

                    result[pattern.Key] = string.Join(", ", values);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a formatted summary of a WHOIS response.
        /// </summary>
        public static string CreateSummary(string rawResponse)
        {
            var keyInfo = ExtractKeyInfo(rawResponse);
            
            if (keyInfo.Count == 0)
            {
                return rawResponse; // Return the original if we couldn't extract info
            }

            var summary = new System.Text.StringBuilder();
            
            foreach (var item in keyInfo)
            {
                summary.AppendLine($"{item.Key}: {item.Value}");
            }

            summary.AppendLine("\n--- Full Response ---\n");
            summary.AppendLine(rawResponse);
            
            return summary.ToString();
        }
    }
}