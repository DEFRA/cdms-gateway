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
        var client = clientFactory.CreateClient(Proxy.ProxyClient);
        var request = new HttpRequestMessage(new HttpMethod(healthUrl.Method), healthUrl.Url);
        var response = await client.SendAsync(request);
        
        var hostname = new Uri(healthUrl.Url).Host;
        var traceRoute = GetTraceRoute(hostname);
        
        return FormatTraceRoute(healthUrl, response.StatusCode, hostname, traceRoute);
    }

    private static IEnumerable<string> GetTraceRoute(string hostname)
    {
        const int Timeout = 10000;
        const int MaxTTL = 30;
        const int BufferSize = 32;

        var buffer = new byte[BufferSize];
        new Random().NextBytes(buffer);
        var stopwatch = new Stopwatch();
        var hopNum = 1;

        using var pinger = new Ping();
        for (var ttl = 1; ttl <= MaxTTL; ttl++)
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
        return $"{hopNum,3}  {GetHostEntry(reply)} [{reply.Address}]  {stopwatch.Elapsed.TotalMilliseconds:0.###} ms";
    }

    private static string FormatTraceRoute(HealthUrl healthUrl, HttpStatusCode statusCode, string hostname, IEnumerable<string> traceRoute)
    {
        return $@"{healthUrl.Name} - {healthUrl.Method} {healthUrl.Url} - {statusCode}
traceroute to {hostname} [{GetHostAddresses(hostname)}], 30 hops max
{string.Join("\r", traceRoute)}
";
    }

    private static string GetHostEntry(PingReply reply)
    {
        try
        {
            return Dns.GetHostEntry(reply.Address).HostName;
        }
        catch
        {
            return "* * *";
        }
    }

    private static string GetHostAddresses(string hostname)
    {
        try
        {
            return string.Join(' ', Dns.GetHostAddresses(hostname).Select(x => x.ToString()));
        }
        catch
        {
            return "*";
        }
    }
}