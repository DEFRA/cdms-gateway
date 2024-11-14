using System.Net;

namespace CdmsGateway.Services.Checking;

public static class DnsResolution
{
    public static IPHostEntry? GetHostEntry(string ipAddress)
    {
        try
        {
            return Dns.GetHostEntry(ipAddress);
        }
        catch
        {
            return null;
        }
    }

    public static IPAddress[] GetHostAddresses(string hostname)
    {
        try
        {
            return Dns.GetHostAddresses(hostname);
        }
        catch
        {
            return [];
        }
    }
}