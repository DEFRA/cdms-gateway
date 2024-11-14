using System.Net;
using System.Net.NetworkInformation;
using CdmsGateway.Services.Routing;

namespace CdmsGateway.Services.Checking;

public class CheckRouteResult
{
    private readonly HealthUrl _healthUrl;
    private int _hopNum = 1;

    public string RouteName => _healthUrl.Name;
    public string RouteMethod => _healthUrl.Method;
    public string RouteUrl => _healthUrl.Url;
    public string RequestResponse { get; }
    public bool IsValidUrl { get; }
    public string[] IpAddresses { get; } = [];
    public string HostName { get; } = string.Empty;
    public List<HopResult> HopResults { get; } = new();

    public CheckRouteResult(HealthUrl healthUrl, string requestResponse)
    {
        _healthUrl = healthUrl;
        RequestResponse = requestResponse;
        IsValidUrl = Uri.TryCreate(healthUrl.Url, UriKind.Absolute, out var uri);
        if (!IsValidUrl) return;
        
        HostName = uri!.Host;
        IpAddresses = DnsResolution.GetHostAddresses(HostName).Select(x => x.ToString()).ToArray();
        if (IpAddresses.FirstOrDefault() == HostName)
            HostName = DnsResolution.GetHostEntry(HostName)?.HostName ?? HostName;
    }

    public void AddHopResult(PingReply? reply, TimeSpan elapsedTime)
    {
        HopResults.Add(new HopResult(_hopNum++, reply?.Address, elapsedTime));
    }
}

public class HopResult
{
    public int HopNum { get; }
    public IPAddress? IpAddress { get; }
    public TimeSpan Elapsed { get; }
    public IPHostEntry? Host { get; }

    public HopResult(int hopNum, IPAddress? ipAddress, TimeSpan elapsed)
    {
        HopNum = hopNum;
        IpAddress = ipAddress;
        Elapsed = elapsed;
        Host = ipAddress != null ? DnsResolution.GetHostEntry(ipAddress.ToString()) : null;
    }
}