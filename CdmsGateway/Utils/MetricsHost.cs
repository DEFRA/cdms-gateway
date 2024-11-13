using System.Diagnostics.Metrics;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Utils;

public class MetricsHost
{
    private readonly ILogger _logger;
    public const string Name = "Cdms.Gateway";

    public readonly Counter<long> RequestRouted;
    public readonly Counter<long> RequestForked;
    public readonly Histogram<long> TotalRequestDuration;
    public readonly Histogram<long> RoutedRequestDuration;
    public readonly Histogram<long> ForkedRequestDuration;

    public MetricsHost(IMeterFactory meterFactory, ILogger logger)
    {
        _logger = logger;
        try
        {
            var meter = meterFactory.Create(Name);
            RequestRouted = meter.CreateCounter<long>("cdms.gateway.routed", "requests", "Number of routed requests made");
            RequestForked = meter.CreateCounter<long>("cdms.gateway.forked", "requests", "Number of forked requests made");
            TotalRequestDuration = meter.CreateHistogram<long>("cdms.gateway.duration.total", "ms", "Duration of routing from receiving request to returning routed response");
            RoutedRequestDuration = meter.CreateHistogram<long>("cdms.gateway.duration.routed", "ms", "Duration of routed request/response");
            ForkedRequestDuration = meter.CreateHistogram<long>("cdms.gateway.duration.forked", "ms", "Duration of forked request/response");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unable to configure metrics");
        }
    }

    public Metrics GetMetrics() => new(this, _logger);
}