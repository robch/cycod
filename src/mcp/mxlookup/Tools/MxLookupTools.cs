using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using DnsClient;
using CycodMcp.MxLookup.Tools;

[McpServerToolType]
public sealed class MxLookupTools
{
    [McpServerTool, Description("Looks up MX records for a domain.")]
    public static async Task<string> LookupMxRecords(
        [Description("The domain name to look up (e.g., example.com).")] string domain)
    {
        try
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domain, QueryType.MX);
            
            // Format the result into a readable format
            return MxLookupHelper.FormatMxResponse(result);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Performs a DNS lookup for all records of a specified domain.")]
    public static async Task<string> LookupAllRecords(
        [Description("The domain name to look up (e.g., example.com).")] string domain)
    {
        try
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domain, QueryType.ANY);
            
            return MxLookupHelper.FormatDnsResponse(result);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Performs a DNS lookup for TXT records of a specified domain.")]
    public static async Task<string> LookupTxtRecords(
        [Description("The domain name to look up (e.g., example.com).")] string domain)
    {
        try
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domain, QueryType.TXT);
            
            return MxLookupHelper.FormatTxtResponse(result);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Performs a DNS lookup for SPF record of a specified domain.")]
    public static async Task<string> LookupSpfRecord(
        [Description("The domain name to look up (e.g., example.com).")] string domain)
    {
        try
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domain, QueryType.TXT);
            
            return MxLookupHelper.FormatSpfResponse(result);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Performs a DNS lookup for DMARC record of a specified domain.")]
    public static async Task<string> LookupDmarcRecord(
        [Description("The domain name to look up (e.g., example.com).")] string domain)
    {
        try
        {
            var lookup = new LookupClient();
            // DMARC records are stored as TXT records at _dmarc.domain.com
            var dmarcDomain = $"_dmarc.{domain}";
            var result = await lookup.QueryAsync(dmarcDomain, QueryType.TXT);
            
            return MxLookupHelper.FormatDmarcResponse(result, domain);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Performs a DNS lookup for A and AAAA records of a specified domain.")]
    public static async Task<string> LookupAddressRecords(
        [Description("The domain name to look up (e.g., example.com).")] string domain)
    {
        try
        {
            var lookup = new LookupClient();
            var ipv4Result = await lookup.QueryAsync(domain, QueryType.A);
            var ipv6Result = await lookup.QueryAsync(domain, QueryType.AAAA);
            
            return MxLookupHelper.FormatAddressResponse(ipv4Result, ipv6Result);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}