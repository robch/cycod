using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net;
using Whois;
using CycodMcp.Whois.Tools;

[McpServerToolType]
public sealed class WhoisTools
{
    [McpServerTool, Description("Looks up whois information about a domain.")]
    public static async Task<string> WhoisDomain(
        [Description("The domain name to look up (e.g., example.com).")] string domain)
    {
        try
        {
            var whoisLookup = new WhoisLookup();
            var response = await whoisLookup.LookupAsync(domain);
            
            // Format the result into a readable format
            return FormatWhoisResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Looks up whois information about a Top Level Domain (TLD).")]
    public static async Task<string> WhoisTld(
        [Description("The TLD to look up (e.g., 'com', 'net', 'org').")] string tld)
    {
        try
        {
            if (!tld.StartsWith("."))
            {
                tld = $".{tld}";
            }

            var whoisLookup = new WhoisLookup();
            var response = await whoisLookup.LookupAsync(tld);
            
            return FormatWhoisResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Looks up whois information about an IP address.")]
    public static async Task<string> WhoisIp(
        [Description("The IP address to look up.")] string ip)
    {
        try
        {
            // Validate the IP address
            if (!IPAddress.TryParse(ip, out _))
            {
                return "Error: Invalid IP address format.";
            }

            var whoisLookup = new WhoisLookup();
            var response = await whoisLookup.LookupAsync(ip);
            
            return FormatWhoisResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Looks up whois information about an Autonomous System Number (ASN).")]
    public static async Task<string> WhoisAs(
        [Description("The ASN to look up (e.g., 'AS123').")] string asn)
    {
        try
        {
            // Ensure ASN is in correct format - "ASxxxx"
            if (!asn.StartsWith("AS", StringComparison.OrdinalIgnoreCase))
            {
                asn = $"AS{asn}";
            }

            var whoisLookup = new WhoisLookup();
            var response = await whoisLookup.LookupAsync(asn);
            
            return FormatWhoisResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Helper method to format the whois response into a readable format
    private static string FormatWhoisResponse(WhoisResponse response)
    {
        if (response == null || string.IsNullOrWhiteSpace(response.Content))
        {
            return "No information found.";
        }

        return WhoisHelper.CreateSummary(response.Content);
    }
}