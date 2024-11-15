using CdmsGateway.Services.Routing;

namespace CdmsGateway.Services.Checking;

public class CheckRouteResult(HealthUrl healthUrl, string responseResult, TimeSpan elapsed)
{
    public string RouteName => healthUrl.Name;
    public string RouteMethod => healthUrl.Method;
    public string RouteUrl => healthUrl.Url;
    public string? ResponseResult { get; } = responseResult;
    public TimeSpan Elapsed { get; } = elapsed;
}
