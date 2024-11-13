using System.Diagnostics.Metrics;

namespace CdmsGateway.Utils;

public class MetricsHost
{
    public const string Name = "Cdms.Gateway";

    public readonly Counter<long> RequestRouted;
    public readonly Counter<long> RequestForked;
    public readonly Histogram<long> TotalRequestDuration;
    public readonly Histogram<long> RoutedRequestDuration;
    public readonly Histogram<long> ForkedRequestDuration;

    public MetricsHost(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(Name);
        RequestRouted = meter.CreateCounter<long>("cdms.gateway.routed", "requests", "Number of routed requests made");
        RequestForked = meter.CreateCounter<long>("cdms.gateway.forked", "requests", "Number of forked requests made");
        TotalRequestDuration = meter.CreateHistogram<long>("cdms.gateway.duration.total", "ms", "Duration of routing from receiving request to returning routed response");
        RoutedRequestDuration = meter.CreateHistogram<long>("cdms.gateway.duration.routed", "ms", "Duration of routed request/response");
        ForkedRequestDuration = meter.CreateHistogram<long>("cdms.gateway.duration.forked", "ms", "Duration of forked request/response");
    }

    public Metrics GetMetrics() => new(this);
}