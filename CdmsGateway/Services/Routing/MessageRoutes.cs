using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services.Routing;

public interface IMessageRoutes
{
    RoutingResult GetReturnedRoute(string routePath);
}

public class MessageRoutes : IMessageRoutes
{
    private readonly ILogger _logger;
    private const string TestName = "test";
    private readonly IDictionary<string, string> _returnedRoutes;
    private readonly IDictionary<string, string> _unreturnedRoutes;

    public MessageRoutes(RoutingConfig routingConfig, ILogger logger)
    {
        _logger = logger;
        try
        {
            if (routingConfig.AllReturnedRoutes.Length != routingConfig.AllReturnedRoutes.Select(x => x.Name).Distinct().Count()) throw new InvalidDataException("Duplicate returned route name");
            if (routingConfig.AllUnreturnedRoutes.Length != routingConfig.AllUnreturnedRoutes.Select(x => x.Name).Distinct().Count()) throw new InvalidDataException("Duplicate unreturned route name");
            if (routingConfig.AllReturnedRoutes.Any(x => !Uri.TryCreate(x.Url, UriKind.Absolute, out _))) throw new InvalidDataException("Returned route URL invalid");
            if (routingConfig.AllUnreturnedRoutes.Any(x => !Uri.TryCreate(x.Url, UriKind.Absolute, out _))) throw new InvalidDataException("Returned route URL invalid");

            _returnedRoutes = routingConfig.AllReturnedRoutes.ToDictionary(x => x.Name.ToLower(), x => x.Url.Trim('/'));
            _unreturnedRoutes = routingConfig.AllUnreturnedRoutes.ToDictionary(x => x.Name.ToLower(), x => x.Url.Trim('/'));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating routing table");
            throw;
        }
    }

    public RoutingResult GetReturnedRoute(string routePath) => GetRoute(routePath, _returnedRoutes);

    public RoutingResult GetUnreturnedRoute(string routePath)=> GetRoute(routePath, _unreturnedRoutes);

    private RoutingResult GetRoute(string routePath, IDictionary<string, string> routes)
    {
        try
        {
            var routeParts = routePath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (routeParts.Length == 0) return new RoutingResult();
        
            var routeName = routeParts[0].ToLower();
            var routeUrl = routes.TryGetValue(routeName, out var url) ? $"{url}/{string.Join('/', routeParts[1..])}" : null;

            return new RoutingResult { RouteFound = routeUrl != null, RouteName = routeName, RouteUrl = routeUrl };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting route");
            throw;
        }
    }
}
