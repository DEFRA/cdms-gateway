using CdmsGateway.Services.Routing;

namespace CdmsGateway.Services.Checking;

public class CheckRouteResult
{
    private readonly HealthUrl _healthUrl;

    public string RouteName => _healthUrl.Name;
    public string RouteMethod => _healthUrl.Method;
    public string RouteUrl => _healthUrl.Url;
    public string? ResponseResult { get; }

    public CheckRouteResult(HealthUrl healthUrl, string responseResult)
    {
        ResponseResult = responseResult;
        _healthUrl = healthUrl;
    }
}
