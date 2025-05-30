using DnsClient;
using DnsClient.Protocol;
using System.Linq;
using System.Text;

namespace CycodMcp.MxLookup.Tools
{
    /// <summary>
    /// Helper class for formatting DNS responses.
    /// </summary>
    internal static class MxLookupHelper
    {
        /// <summary>
        /// Formats MX record responses into a readable string.
        /// </summary>
        public static string FormatMxResponse(IDnsQueryResponse response)
        {
            if (response == null || !response.Answers.Any())
            {
                return "No MX records found for this domain.";
            }

            var mxRecords = response.Answers.MxRecords().OrderBy(mx => mx.Preference).ToList();
            
            if (!mxRecords.Any())
            {
                return "No MX records found for this domain.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("MX Records:");
            sb.AppendLine("----------------------------------------------------------");
            sb.AppendLine("| Priority | Mail Server                                 |");
            sb.AppendLine("----------------------------------------------------------");
            
            foreach (var record in mxRecords)
            {
                sb.AppendLine($"| {record.Preference,-8} | {record.Exchange,-43} |");
            }
            
            sb.AppendLine("----------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine("Note: Mail servers are listed in priority order (lower number = higher priority).");
            
            return sb.ToString();
        }

        /// <summary>
        /// Formats DNS response for ANY query into a readable string.
        /// </summary>
        public static string FormatDnsResponse(IDnsQueryResponse response)
        {
            if (response == null || !response.Answers.Any())
            {
                return "No DNS records found for this domain.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("DNS Records:");
            sb.AppendLine("------------------------------------------------------");
            
            // Group records by type
            var recordGroups = response.Answers.GroupBy(r => r.RecordType);
            
            foreach (var group in recordGroups)
            {
                sb.AppendLine($"Record Type: {group.Key}");
                sb.AppendLine("------------------------------------------------------");
                
                foreach (var record in group)
                {
                    sb.AppendLine(FormatDnsRecord(record));
                }
                
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Formats TXT record responses into a readable string.
        /// </summary>
        public static string FormatTxtResponse(IDnsQueryResponse response)
        {
            if (response == null || !response.Answers.Any())
            {
                return "No TXT records found for this domain.";
            }

            var txtRecords = response.Answers.TxtRecords().ToList();
            
            if (!txtRecords.Any())
            {
                return "No TXT records found for this domain.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("TXT Records:");
            sb.AppendLine("------------------------------------------------------");
            
            foreach (var record in txtRecords)
            {
                sb.AppendLine($"TTL: {record.TimeToLive}");
                sb.AppendLine($"Text: {string.Join(" ", record.Text)}");
                sb.AppendLine("------------------------------------------------------");
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Formats SPF record responses into a readable string.
        /// </summary>
        public static string FormatSpfResponse(IDnsQueryResponse response)
        {
            if (response == null || !response.Answers.Any())
            {
                return "No SPF records found for this domain.";
            }

            var txtRecords = response.Answers.TxtRecords().ToList();
            
            if (!txtRecords.Any())
            {
                return "No TXT records found for this domain.";
            }

            // Find SPF record
            var spfRecord = txtRecords.FirstOrDefault(r => 
                r.Text.Any(txt => txt.StartsWith("v=spf1", StringComparison.OrdinalIgnoreCase)));
            
            if (spfRecord == null)
            {
                return "No SPF record found for this domain.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("SPF Record:");
            sb.AppendLine("------------------------------------------------------");
            sb.AppendLine($"TTL: {spfRecord.TimeToLive}");
            sb.AppendLine($"SPF: {string.Join(" ", spfRecord.Text)}");
            sb.AppendLine("------------------------------------------------------");
            sb.AppendLine();
            
            // Provide a simple explanation of the SPF record
            sb.AppendLine("Explanation:");
            var spfText = string.Join(" ", spfRecord.Text);
            
            if (spfText.Contains("~all"))
            {
                sb.AppendLine("This SPF record uses '~all' which means a soft fail policy. " +
                              "Messages that fail the SPF check will be accepted but may be marked.");
            }
            else if (spfText.Contains("-all"))
            {
                sb.AppendLine("This SPF record uses '-all' which means a hard fail policy. " +
                              "Messages that fail the SPF check should be rejected.");
            }
            else if (spfText.Contains("?all"))
            {
                sb.AppendLine("This SPF record uses '?all' which means a neutral policy. " +
                              "The SPF check will have no effect on message delivery.");
            }
            else if (spfText.Contains("+all"))
            {
                sb.AppendLine("This SPF record uses '+all' which means a pass policy. " +
                              "All messages will pass the SPF check, regardless of source.");
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Formats DMARC record responses into a readable string.
        /// </summary>
        public static string FormatDmarcResponse(IDnsQueryResponse response, string originalDomain)
        {
            if (response == null || !response.Answers.Any())
            {
                return $"No DMARC record found for _dmarc.{originalDomain}.";
            }

            var txtRecords = response.Answers.TxtRecords().ToList();
            
            if (!txtRecords.Any())
            {
                return $"No DMARC record found for _dmarc.{originalDomain}.";
            }

            // Find DMARC record
            var dmarcRecord = txtRecords.FirstOrDefault(r => 
                r.Text.Any(txt => txt.StartsWith("v=DMARC1", StringComparison.OrdinalIgnoreCase)));
            
            if (dmarcRecord == null)
            {
                return $"No DMARC record found for _dmarc.{originalDomain}.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("DMARC Record:");
            sb.AppendLine("------------------------------------------------------");
            sb.AppendLine($"TTL: {dmarcRecord.TimeToLive}");
            sb.AppendLine($"DMARC: {string.Join(" ", dmarcRecord.Text)}");
            sb.AppendLine("------------------------------------------------------");
            sb.AppendLine();
            
            // Parse and explain DMARC tags
            var dmarcText = string.Join(" ", dmarcRecord.Text);
            var tags = ParseDmarcTags(dmarcText);
            
            if (tags.Count > 0)
            {
                sb.AppendLine("DMARC Policy Explanation:");
                sb.AppendLine("------------------------------------------------------");
                
                // Explain 'p' tag
                if (tags.TryGetValue("p", out var policy))
                {
                    sb.Append("Policy (p): ");
                    switch (policy.ToLower())
                    {
                        case "none":
                            sb.AppendLine("Monitor mode. No specific action is requested.");
                            break;
                        case "quarantine":
                            sb.AppendLine("Messages that fail should be quarantined (usually sent to spam folder).");
                            break;
                        case "reject":
                            sb.AppendLine("Messages that fail should be rejected outright.");
                            break;
                        default:
                            sb.AppendLine(policy);
                            break;
                    }
                }
                
                // Explain 'sp' tag if present
                if (tags.TryGetValue("sp", out var subPolicy))
                {
                    sb.Append("Subdomain Policy (sp): ");
                    switch (subPolicy.ToLower())
                    {
                        case "none":
                            sb.AppendLine("Monitor mode for subdomains. No specific action is requested.");
                            break;
                        case "quarantine":
                            sb.AppendLine("Messages from subdomains that fail should be quarantined.");
                            break;
                        case "reject":
                            sb.AppendLine("Messages from subdomains that fail should be rejected.");
                            break;
                        default:
                            sb.AppendLine(subPolicy);
                            break;
                    }
                }
                
                // Explain 'pct' tag if present
                if (tags.TryGetValue("pct", out var percent))
                {
                    sb.AppendLine($"Percentage (pct): {percent}% of messages are subject to filtering.");
                }
                
                // Explain reporting options if present
                if (tags.TryGetValue("rua", out var rua))
                {
                    sb.AppendLine($"Aggregate Reports (rua): Reports sent to {rua}");
                }
                
                if (tags.TryGetValue("ruf", out var ruf))
                {
                    sb.AppendLine($"Forensic Reports (ruf): Reports sent to {ruf}");
                }
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Formats A and AAAA record responses into a readable string.
        /// </summary>
        public static string FormatAddressResponse(IDnsQueryResponse ipv4Response, IDnsQueryResponse ipv6Response)
        {
            var sb = new StringBuilder();
            
            // Format IPv4 addresses
            var aRecords = ipv4Response?.Answers?.ARecords()?.ToList();
            if (aRecords != null && aRecords.Any())
            {
                sb.AppendLine("IPv4 Addresses (A Records):");
                sb.AppendLine("------------------------------------------------------");
                
                foreach (var record in aRecords)
                {
                    sb.AppendLine($"{record.Address}  (TTL: {record.TimeToLive})");
                }
                
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("No IPv4 addresses (A records) found.");
                sb.AppendLine();
            }
            
            // Format IPv6 addresses
            var aaaaRecords = ipv6Response?.Answers?.AaaaRecords()?.ToList();
            if (aaaaRecords != null && aaaaRecords.Any())
            {
                sb.AppendLine("IPv6 Addresses (AAAA Records):");
                sb.AppendLine("------------------------------------------------------");
                
                foreach (var record in aaaaRecords)
                {
                    sb.AppendLine($"{record.Address}  (TTL: {record.TimeToLive})");
                }
            }
            else
            {
                sb.AppendLine("No IPv6 addresses (AAAA records) found.");
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Formats a generic DNS record into a readable string.
        /// </summary>
        private static string FormatDnsRecord(DnsResourceRecord record)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"Name: {record.DomainName}");
            sb.AppendLine($"TTL: {record.TimeToLive}");
            
            switch (record)
            {
                case ARecord aRecord:
                    sb.AppendLine($"IP: {aRecord.Address}");
                    break;
                    
                case AaaaRecord aaaaRecord:
                    sb.AppendLine($"IPv6: {aaaaRecord.Address}");
                    break;
                    
                case MxRecord mxRecord:
                    sb.AppendLine($"Preference: {mxRecord.Preference}");
                    sb.AppendLine($"Exchange: {mxRecord.Exchange}");
                    break;
                    
                case TxtRecord txtRecord:
                    sb.AppendLine($"Text: {string.Join(" ", txtRecord.Text)}");
                    break;
                    
                case NsRecord nsRecord:
                    sb.AppendLine($"NameServer: {nsRecord.NSDName}");
                    break;
                    
                case CNameRecord cnameRecord:
                    sb.AppendLine($"CanonicalName: {cnameRecord.CanonicalName}");
                    break;
                    
                case SoaRecord soaRecord:
                    sb.AppendLine($"MName: {soaRecord.MName}");
                    sb.AppendLine($"RName: {soaRecord.RName}");
                    sb.AppendLine($"Serial: {soaRecord.Serial}");
                    sb.AppendLine($"Refresh: {soaRecord.Refresh}");
                    sb.AppendLine($"Retry: {soaRecord.Retry}");
                    sb.AppendLine($"Expire: {soaRecord.Expire}");
                    sb.AppendLine($"Minimum: {soaRecord.Minimum}");
                    break;
                    
                default:
                    sb.AppendLine($"Record Type: {record.RecordType}");
                    break;
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Parse DMARC tags into a dictionary.
        /// </summary>
        private static Dictionary<string, string> ParseDmarcTags(string dmarcRecord)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            if (string.IsNullOrEmpty(dmarcRecord))
            {
                return result;
            }
            
            // Split the record into tags
            var tags = dmarcRecord.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var tag in tags)
            {
                var parts = tag.Trim().Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    
                    result[key] = value;
                }
            }
            
            return result;
        }
    }
}