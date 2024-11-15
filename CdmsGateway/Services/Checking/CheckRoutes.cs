using System.Diagnostics;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils.Http;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services.Checking;

public class CheckRoutes(IMessageRoutes messageRoutes, IHttpClientFactory clientFactory, ILogger logger)
{
    public const int OverallTimeoutSecs = 50;

    public async Task<IEnumerable<CheckRouteResult>> Check()
    {
        logger.Information("Start route checking");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(OverallTimeoutSecs));
        return await Task.WhenAll(messageRoutes.HealthUrls.Select(healthUrl => Check(healthUrl, cts)));
    }

    private async Task<CheckRouteResult> Check(HealthUrl healthUrl, CancellationTokenSource cts)
    {
        CheckRouteResult checkRouteResult;
        var stopwatch = new Stopwatch();

        try
        {
            logger.Information("Start checking HTTP request for {Url}", healthUrl.Url);
            var client = clientFactory.CreateClient(Proxy.ProxyClientWithoutRetry);
            var request = new HttpRequestMessage(new HttpMethod(healthUrl.Method), healthUrl.Url);
            stopwatch.Start();
            var response = await client.SendAsync(request, cts.Token);
            checkRouteResult = new CheckRouteResult(healthUrl, $"{response.StatusCode.ToString()} ({(int)response.StatusCode})", stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            checkRouteResult = new CheckRouteResult(healthUrl, $"\"{ex.Message}\"", stopwatch.Elapsed);
        }
        
        stopwatch.Stop();
        logger.Information("Completed checking HTTP request for {Url} with result {Result}", healthUrl.Url, checkRouteResult.ResponseResult);
        
        return checkRouteResult;
    }
}
