namespace CdmsGateway.Services.Checking;

public static class CheckRouteResultExtensions
{
    public static string FormatTraceRoutes(this IEnumerable<CheckRouteResult> results) => string.Join('\r', results.Select(result => result.IsValidUrl ? result.FormatValidTraceRoute() : result.FormatInvalidTraceRoute()));

    private static string FormatInvalidTraceRoute(this CheckRouteResult result) => $"{result.FormatTraceRouteFirstLine()}\r" +
                                                                                   $"Unable to perform trace due to invalid URL\r";

    private static string FormatValidTraceRoute(this CheckRouteResult result) => $"{result.FormatTraceRouteFirstLine()}\r" +
                                                                                 $"Trace to {result.HostName} [{string.Join(' ', result.IpAddresses)}], " +
                                                                                 $"{CheckRoutes.MaxHops} hops max, {CheckRoutes.Timeout/1000:0.###} sec timeout.  " +
                                                                                 $"Total elapsed {result.HopResults.Sum(x => x.Elapsed.TotalMilliseconds):0.###} ms\r" +
                                                                                 $"{string.Join('\r', result.HopResults.Select(FormatHopResult))}\r";

    private static string FormatHopResult(this HopResult result) => $"{result.HopNum,3}  {result.Host?.HostName ?? "* * *"} [{result.IpAddress?.ToString() ?? "* * *"}]  {result.Elapsed.TotalMilliseconds:0.###} ms";

    private static string FormatTraceRouteFirstLine(this CheckRouteResult result) => $"{result.RouteName} - {result.RouteMethod} {result.RouteUrl} - {result.RequestResponse}";
}