using System.Diagnostics;
using System.Net.NetworkInformation;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils.Http;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services.Checking;

public class CheckRoutes(IMessageRoutes messageRoutes, IHttpClientFactory clientFactory, ILogger logger)
{
    public const int Timeout = 2000;
    public const int MaxHops = 20;

    public async Task<IEnumerable<CheckRouteResult>> Check()
    {
        logger.Information("Start route checking");
        return await Task.WhenAll(messageRoutes.HealthUrls.Select(Check));
    }

    private async Task<CheckRouteResult> Check(HealthUrl healthUrl)
    {
        var checkRouteResult = new CheckRouteResult(healthUrl);

        await Task.WhenAll(
            CheckHttpRequest(healthUrl, checkRouteResult),
            CheckIpRouting(checkRouteResult));
        
        return checkRouteResult;
    }

    private async Task CheckHttpRequest(HealthUrl healthUrl, CheckRouteResult checkRouteResult)
    {
        try
        {
            logger.Information("Start checking HTTP request for {Url}", healthUrl.Url);
            var client = clientFactory.CreateClient(Proxy.ProxyClientWithoutRetry);
            var request = new HttpRequestMessage(new HttpMethod(healthUrl.Method), healthUrl.Url);
            var response = await client.SendAsync(request);
            checkRouteResult.ResponseResult = response.StatusCode.ToString();
        }
        catch (Exception ex)
        {
            checkRouteResult.ResponseResult = $"\"{ex.Message}\"";
        }
        
        logger.Information("Completed checking HTTP request for {Url} with result {Result}", healthUrl.Url, checkRouteResult.ResponseResult);
    }

    private Task CheckIpRouting(CheckRouteResult checkRouteResult)
    {
        if (checkRouteResult.IsValidUrl)
        {
            logger.Information("Start discovering trace for {Host}", checkRouteResult.HostName);
            foreach (var hopResult in GetTraceRoute(checkRouteResult.HostName))
                checkRouteResult.AddHopResult(hopResult.Reply, hopResult.Elapsed);
            logger.Information("Completed discovering trace for {Host} in {Elapsed} ms", checkRouteResult.HostName, checkRouteResult.HopResults.Sum(x => x.Elapsed.TotalMicroseconds));
        }

        return Task.CompletedTask;
    }

    private IEnumerable<(PingReply? Reply, TimeSpan Elapsed)> GetTraceRoute(string hostname)
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
                logger.Information("Successfully pinged {Hop} {Host} in {Elapsed} ms", ttl, hostname, stopwatch.Elapsed.TotalMicroseconds);
            }
            catch
            {
                stopwatch.Stop();
                logger.Information("Failed to ping {Hop} {Host} in {Elapsed} ms", ttl, hostname, stopwatch.Elapsed.TotalMicroseconds);
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
