using System.Diagnostics;
using System.Net.NetworkInformation;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils.Http;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services.Checking;

public class CheckRoutes(IMessageRoutes messageRoutes, IHttpClientFactory clientFactory, ILogger logger)
{
    public const int OverallTimeoutSecs = 30;
    public const int HopTimeoutMs = 3000;
    public const int MaxHops = 20;

    public async Task<IEnumerable<CheckRouteResult>> Check()
    {
        logger.Information("Start route checking");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(OverallTimeoutSecs));
        return await Task.WhenAll(messageRoutes.HealthUrls.Select(healthUrl => Check(healthUrl, cts)));
    }

    private async Task<CheckRouteResult> Check(HealthUrl healthUrl, CancellationTokenSource cts)
    {
        var checkRouteResult = new CheckRouteResult(healthUrl);

        await Task.WhenAll(
            CheckHttpRequest(healthUrl, checkRouteResult, cts),
            CheckIpRouting(checkRouteResult, cts));
        
        return checkRouteResult;
    }

    private async Task CheckHttpRequest(HealthUrl healthUrl, CheckRouteResult checkRouteResult, CancellationTokenSource cts)
    {
        try
        {
            logger.Information("Start checking HTTP request for {Url}", healthUrl.Url);
            var client = clientFactory.CreateClient(Proxy.ProxyClientWithoutRetry);
            var request = new HttpRequestMessage(new HttpMethod(healthUrl.Method), healthUrl.Url);
            var response = await client.SendAsync(request, cts.Token);
            checkRouteResult.ResponseResult = response.StatusCode.ToString();
        }
        catch (Exception ex)
        {
            checkRouteResult.ResponseResult = $"\"{ex.Message}\"";
        }
        
        logger.Information("Completed checking HTTP request for {Url} with result {Result}", healthUrl.Url, checkRouteResult.ResponseResult);
    }

    private Task CheckIpRouting(CheckRouteResult checkRouteResult, CancellationTokenSource cts)
    {
        if (checkRouteResult.IsValidUrl)
        {
            var cancellationToken = cts.Token;
            logger.Information("Start discovering trace for {Host}", checkRouteResult.HostName);
            foreach (var hopResult in GetTraceRoute(checkRouteResult.HostName))
            {
                checkRouteResult.AddHopResult(hopResult.Reply, hopResult.Elapsed);
                if (cancellationToken.IsCancellationRequested) break;
            }
            logger.Information("Completed discovering trace for {Host}", checkRouteResult.HostName);
        }

        return Task.CompletedTask;
    }

    private IEnumerable<(PingReply? Reply, TimeSpan Elapsed)> GetTraceRoute(string hostname)
    {
        const int BufferSize = 32;
        const int MaxPingErrors = 3;

        var buffer = new byte[BufferSize];
        new Random().NextBytes(buffer);
        var stopwatch = new Stopwatch();
        var pingErrors = 0;

        using var pinger = new Ping();
        for (var ttl = 1; ttl <= MaxHops; ttl++)
        {
            var options = new PingOptions(ttl, true);

            PingReply? reply;
            try
            {
                stopwatch.Restart();
                reply = pinger.Send(hostname, HopTimeoutMs, buffer, options);
                stopwatch.Stop();
                pingErrors += reply.Status == IPStatus.TimedOut ? 1 : 0;
                logger.Information("Successfully pinged {Hop} {Host}", ttl, hostname);
            }
            catch
            {
                stopwatch.Stop();
                logger.Information("Failed to ping {Hop} {Host}", ttl, hostname);
                pingErrors++;
                reply = null;
            }
            if (pingErrors > MaxPingErrors) break;
            
            // we've found a route at this ttl
            if (reply is not { Status: not (IPStatus.Success or IPStatus.TtlExpired) })
                yield return (reply, stopwatch.Elapsed);

            // if we reach a status other than expired or timed out, we're done searching or there has been an error
            if (reply is { Status: not IPStatus.TtlExpired and not IPStatus.TimedOut })
                break;
        }
    }
}
