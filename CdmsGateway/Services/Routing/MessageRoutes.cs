using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services.Routing;

public interface IMessageRoutes
{
    RoutingResult GetRoutedRoute(string routePath);
    RoutingResult GetForkedRoute(string routePath);
}

public class MessageRoutes : IMessageRoutes
{
    private readonly ILogger _logger;
    private readonly IDictionary<string, string> _routedRoutes;
    private readonly IDictionary<string, string> _forkedRoutes;

    public MessageRoutes(RoutingConfig routingConfig, ILogger logger)
    {
        _logger = logger;
        try
        {
            if (routingConfig.AllRoutedRoutes.Length != routingConfig.AllRoutedRoutes.Select(x => x.Name).Distinct().Count()) throw new InvalidDataException("Duplicate routed route name");
            if (routingConfig.AllForkedRoutes.Length != routingConfig.AllForkedRoutes.Select(x => x.Name).Distinct().Count()) throw new InvalidDataException("Duplicate forked route name");
            if (routingConfig.AllRoutedRoutes.Any(x => !Uri.TryCreate(x.Url, UriKind.Absolute, out _))) throw new InvalidDataException("Routed URL invalid");
            if (routingConfig.AllForkedRoutes.Any(x => !Uri.TryCreate(x.Url, UriKind.Absolute, out _))) throw new InvalidDataException("Forked URL invalid");

            _routedRoutes = routingConfig.AllRoutedRoutes.ToDictionary(x => x.Name.ToLower(), x => x.Url.Trim('/'));
            _forkedRoutes = routingConfig.AllForkedRoutes.ToDictionary(x => x.Name.ToLower(), x => x.Url.Trim('/'));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating routing table");
            throw;
        }
    }

    public RoutingResult GetRoutedRoute(string routePath) => GetRoute(routePath, _routedRoutes);

    public RoutingResult GetForkedRoute(string routePath) => GetRoute(routePath, _forkedRoutes);

    private RoutingResult GetRoute(string routePath, IDictionary<string, string> routes)
    {
        try
        {
            var routeParts = routePath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (routeParts.Length == 0) return new RoutingResult();
        
            var routeName = routeParts[0].ToLower();
            var routeUrlPath = $"/{string.Join('/', routeParts[1..])}";
            var routeUrl = routes.TryGetValue(routeName, out var url) ? $"{url}{routeUrlPath}" : null;

            return new RoutingResult { RouteFound = routeUrl != null, RouteName = routeName, RouteUrl = routeUrl, RouteUrlPath = routeUrlPath };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting route");
            throw;
        }
    }
}
