using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils.Http;

namespace CdmsGateway.Services.Checking;

public class CheckRoutes(IMessageRoutes messageRoutes, IHttpClientFactory clientFactory)
{
    public async Task<string> Check()
    {
        var results = await Task.WhenAll(messageRoutes.HealthUrls.Select(Check));
        return string.Join("\r", results);
    }

    private async Task<string> Check(HealthUrl healthUrl)
    {
        string statusCode;
        try
        {
            var client = clientFactory.CreateClient(Proxy.ProxyClient);
            var request = new HttpRequestMessage(new HttpMethod(healthUrl.Method), healthUrl.Url);
            var response = await client.SendAsync(request);
            statusCode = response.StatusCode.ToString();
        }
        catch (Exception ex)
        {
            statusCode = $"\"{ex.Message}\"";
        }

        if (!Uri.TryCreate(healthUrl.Url, UriKind.Absolute, out var uri))
            return FormatInvalidUri(healthUrl, statusCode);
            
        var hostname = uri.Host;
        var traceRoute = GetTraceRoute(hostname);
        
        return FormatTraceRoute(healthUrl, statusCode, hostname, traceRoute);
    }

    private static IEnumerable<string> GetTraceRoute(string hostname)
    {
        const int Timeout = 10000;
        const int MaxTtl = 30;
        const int BufferSize = 32;

        var buffer = new byte[BufferSize];
        new Random().NextBytes(buffer);
        var stopwatch = new Stopwatch();
        var hopNum = 1;

        using var pinger = new Ping();
        for (var ttl = 1; ttl <= MaxTtl; ttl++)
        {
            var options = new PingOptions(ttl, true);
            
            stopwatch.Restart();
            var reply = pinger.Send(hostname, Timeout, buffer, options);
            stopwatch.Stop();

            // we've found a route at this ttl
            if (reply.Status is IPStatus.Success or IPStatus.TtlExpired)
                yield return FormatHopResult(hopNum++, reply, stopwatch);

            // if we reach a status other than expired or timed out, we're done searching or there has been an error
            if (reply.Status != IPStatus.TtlExpired && reply.Status != IPStatus.TimedOut)
                break;
        }
    }

    private static string FormatHopResult(int hopNum, PingReply reply, Stopwatch stopwatch)
    {
        return $"{hopNum,3}  {GetHostEntry(reply.Address.ToString())} [{reply.Address}]  {stopwatch.Elapsed.TotalMilliseconds:0.###} ms";
    }

    private static string FormatInvalidUri(HealthUrl healthUrl, string statusCode)
    {
        return $@"{healthUrl.Name} - {healthUrl.Method} {healthUrl.Url} - {statusCode}
unable to perform traceroute due to invalid URL
";
    }

    private static string FormatTraceRoute(HealthUrl healthUrl, string statusCode, string hostname, IEnumerable<string> traceRoute)
    {
        var ipAddresses = GetHostAddresses(hostname);
        if (ipAddresses.FirstOrDefault() == hostname)
            hostname = GetHostEntry(hostname);
        
        return $@"{healthUrl.Name} - {healthUrl.Method} {healthUrl.Url} - {statusCode}
traceroute to {hostname} [{string.Join(' ', ipAddresses)}], 30 hops max
{string.Join("\r", traceRoute)}
";
    }

    private static string GetHostEntry(string ipAddress)
    {
        try
        {
            return Dns.GetHostEntry(ipAddress).HostName;
        }
        catch
        {
            return "* * *";
        }
    }

    private static string[] GetHostAddresses(string hostname)
    {
        try
        {
            return Dns.GetHostAddresses(hostname).Select(x => x.ToString()).ToArray();
        }
        catch
        {
            return ["*"];
        }
    }
}