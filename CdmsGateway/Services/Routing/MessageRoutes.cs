using CdmsGateway.Config;

namespace CdmsGateway.Services.Routing;

public interface IMessageRoutes
{
    RouteDefinition GetRoute(string from);
}

public class MessageRoutes : IMessageRoutes
{
    private readonly IEnumerable<RouteDefinition> _routes;

    public MessageRoutes(RouteConfig routeConfig)
    {
        _routes = routeConfig.StubbedRoutes.Select(route => new RouteDefinition
        {
            Url = route.Url,
            Path = route.Path, 
            ContentType = route.ContentType
        });
    }

    public RouteDefinition GetRoute(string from)
    {
        return _routes.SingleOrDefault(x => x.Path == from.ToLower().Trim('/')) ?? RouteDefinition.Empty;
    }
}
