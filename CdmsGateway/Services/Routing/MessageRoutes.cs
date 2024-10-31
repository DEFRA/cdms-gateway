namespace CdmsGateway.Services.Routing;

public interface IMessageRoutes
{
    (string Name, string? Url) GetRoute(string routePath);
}

public class MessageRoutes : IMessageRoutes
{
    private const string TestName = "test";
    private readonly IDictionary<string, string> _routes;

    public MessageRoutes(RouteConfig routeConfig)
    {
        var stubUrl = routeConfig.StubUrl.TrimEnd('/');
        _routes = routeConfig.Routes
            .Select(x => new
            {
                Name = x.Name.Trim('/'), 
                Url = x.SelectedRoute switch
                {
                    SelectedRoute.New => x.NewUrl,
                    SelectedRoute.Legacy => x.LegacyUrl,
                    _ => null
                } ?? stubUrl
            })
            .Select(x => x with { Url = $"{x.Url.TrimEnd('/')}/{x.Name.TrimStart('/')}" })
            .Concat([new { Name = TestName, Url = $"{stubUrl}/{TestName}" }])
            .ToDictionary(x => x.Name.ToLower(), x => x.Url.ToLower());
    }

    public (string Name, string? Url) GetRoute(string routePath)
    {
        var routeParts = routePath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (routeParts.Length == 0)
            return default;
        var routeName = routeParts[0].Trim('/').ToLower();

        var routeUrl = _routes.TryGetValue(routeName, out var value) ? $"{value}/{string.Join('/', routeParts[1..])}" : null;
        return (routeName, routeUrl);
    }
}
