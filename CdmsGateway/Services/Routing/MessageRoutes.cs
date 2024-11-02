namespace CdmsGateway.Services.Routing;

public interface IMessageRoutes
{
    RoutingResult GetRoute(string routePath);
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
                Url = (x.SelectedRoute switch
                {
                    SelectedRoute.New => x.NewUrl ?? $"{stubUrl}/{x.Name}",
                    SelectedRoute.Legacy => x.LegacyUrl ?? $"{stubUrl}/{x.Name}",
                    SelectedRoute.Stub => $"{stubUrl}/{x.Name}",
                    _ => $"{stubUrl}/{x.Name}"
                }).TrimEnd('/')
            })
            .Concat([new { Name = TestName, Url = $"{stubUrl}/{TestName}" }])
            .ToDictionary(x => x.Name.ToLower(), x => x.Url.ToLower());
    }

    public RoutingResult GetRoute(string routePath)
    {
        var routeParts = routePath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (routeParts.Length == 0) return new RoutingResult();
        
        var routeName = routeParts[0].Trim('/').ToLower();
        var routeUrl = _routes.TryGetValue(routeName, out var value) ? $"{value}/{string.Join('/', routeParts[1..])}" : null;

        return new RoutingResult { RouteFound = true, RouteName = routeName, RouteUrl = routeUrl };
    }
}
