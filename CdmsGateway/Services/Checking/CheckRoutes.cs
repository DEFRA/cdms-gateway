using System.Diagnostics;
using System.Net.NetworkInformation;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils.Http;

namespace CdmsGateway.Services.Checking;

public class CheckRoutes(IMessageRoutes messageRoutes, IHttpClientFactory clientFactory)
{
    public const int Timeout = 2000;
    public const int MaxHops = 20;

    public async Task<IEnumerable<CheckRouteResult>> Check()
    {
        return await Task.WhenAll(messageRoutes.HealthUrls.Select(Check));
    }

    private async Task<CheckRouteResult> Check(HealthUrl healthUrl)
    {
        string requestResponse;
        try
        {
            var client = clientFactory.CreateClient(Proxy.ProxyClient);
            var request = new HttpRequestMessage(new HttpMethod(healthUrl.Method), healthUrl.Url);
            var response = await client.SendAsync(request);
            requestResponse = response.StatusCode.ToString();
        }
        catch (Exception ex)
        {
            requestResponse = $"\"{ex.Message}\"";
        }

        var checkRouteResult = new CheckRouteResult(healthUrl, requestResponse);
        if (!checkRouteResult.IsValidUrl) return checkRouteResult;

        foreach (var hopResult in GetTraceRoute(checkRouteResult.HostName))
            checkRouteResult.AddHopResult(hopResult.Reply, hopResult.Elapsed);
        
        return checkRouteResult;
    }

    private static IEnumerable<(PingReply? Reply, TimeSpan Elapsed)> GetTraceRoute(string hostname)
    {
        const int BufferSize = 32;

        var buffer = new byte[BufferSize];
        new Random().NextBytes(buffer);
        var stopwatch = new Stopwatch();

        using var pinger = new Ping();
        for (var ttl = 1; ttl <= MaxHops; ttl++)
        {
            var options = new PingOptions(ttl, true);

            PingReply? reply;
            try
            {
                stopwatch.Restart();
                reply = pinger.Send(hostname, Timeout, buffer, options);
                stopwatch.Stop();
            }
            catch
            {
                stopwatch.Stop();
                reply = null;
            }

            // we've found a route at this ttl
            if (reply is not { Status: not (IPStatus.Success or IPStatus.TtlExpired) })
                yield return (reply, stopwatch.Elapsed);

            // if we reach a status other than expired or timed out, we're done searching or there has been an error
            if (reply is { Status: not IPStatus.TtlExpired and not IPStatus.TimedOut })
                break;
        }
    }
}
