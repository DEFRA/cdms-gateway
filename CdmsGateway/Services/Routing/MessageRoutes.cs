using CdmsGateway.Config;

namespace CdmsGateway.Services.Routing;

public interface IMessageRoutes
{
    string? GetRoute(string from);
}

public class MessageRoutes : IMessageRoutes
{
    private const string TestPath = "test";
    private readonly IDictionary<string, string> _routes;

    public MessageRoutes(RouteConfig routeConfig)
    {
        var stubUrl = routeConfig.StubUrl.TrimEnd('/');
        _routes = routeConfig.RealRoutes
            .Select(x => new { Path = x.Path.Trim('/'), Url = $"{(x.Url ?? stubUrl).TrimEnd('/')}/{x.Path.TrimStart('/')}" })
            .Concat([new { Path = TestPath, Url = $"{stubUrl}/{TestPath}" }])
            .ToDictionary(x => x.Path.ToLower(), x => x.Url.ToLower());
    }

    public string? GetRoute(string from)
    {
        return _routes.TryGetValue(from.Trim('/').ToLower(), out var value) ? value : null;
    }
}
