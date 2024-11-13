using System.Diagnostics;
using CdmsGateway.Services;
using CdmsGateway.Services.Routing;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Utils;

public class Metrics(MetricsHost metricsHost, ILogger logger)
{
    public void RequestRouted(MessageData messageData, RoutingResult routingResult)
    {
        try
        {
            metricsHost.RequestRouted.Add(1, CompletedList(messageData, routingResult));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unable to write metrics");
        }
    }

    public void RequestForked(MessageData messageData, RoutingResult routingResult)
    {
        try
        {
            metricsHost.RequestForked.Add(1, CompletedList(messageData, routingResult));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unable to write metrics");
        }
    }

    private static TagList CompletedList(MessageData messageData, RoutingResult routingResult)
    {
        return new TagList
        {
            { "correlation-id", messageData.CorrelationId },
            { "originating-url", messageData.Url },
            { "method", messageData.Method },
            { "content-type", messageData.ContentType },
            { "path", messageData.Path },
            { "route-name", routingResult.RouteName },
            { "route-found", routingResult.RouteFound },
            { "routing-successful", routingResult.RoutingSuccessful },
            { "forward-url", routingResult.RouteUrl },
            { "status-code", routingResult.StatusCode }
        };
    }
    
    // public void StartTotalRequest() => _totalRequestDuration.Start();
    // public void RecordTotalRequest() => metricsHost.TotalRequestDuration.Record(_totalRequestDuration.ElapsedMilliseconds);
    //
    // public void StartRoutedRequest() => _routedRequestDuration.Start();
    // public void RecordRoutedRequest() => metricsHost.RoutedRequestDuration.Record(_routedRequestDuration.ElapsedMilliseconds);
    //
    // public void StartForkedRequest() => _forkedRequestDuration.Start();
    // public void RecordForkedRequest() => metricsHost.ForkedRequestDuration.Record(_forkedRequestDuration.ElapsedMilliseconds);

    private readonly Stopwatch _totalRequestDuration = new();
    private readonly Stopwatch _routedRequestDuration = new();
    private readonly Stopwatch _forkedRequestDuration = new();
}
