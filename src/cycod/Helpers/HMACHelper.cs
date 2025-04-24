using System;
using System.Security.Cryptography;
using System.Text;

public static class HMACHelper
{
    // Computes an HMAC header value using the given key. This implementation uses HMACSHA256 and includes a Unix timestamp.
    public static string Encode(string key)
    {
        // Get current Unix timestamp
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = unixTime.ToString();

        byte[] keyBytes = Encoding.ASCII.GetBytes(key);
        using (var hmac = new HMACSHA256(keyBytes))
        {
            var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(data));
            var hashHex = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            return $"{unixTime}.{hashHex}";
        }
    }
}
