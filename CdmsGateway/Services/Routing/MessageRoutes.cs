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

    public MessageRoutes()
    {
        //var routeConfig = new RouteConfig { StubUrl = "http://cdms-gateway-stub.localtest.me:3092/", Routes = [new SingleRoute { Path = "alvs-apaffs" }, new SingleRoute { Path = "cds" }, new SingleRoute { Path = "alvs-cds" }] };
        var routeConfig = new RouteConfig { StubUrl = "https://cdms-gateway-stub.dev.cdp-int.defra.cloud/", Routes = [new SingleRoute { Path = "alvs-apaffs" }, new SingleRoute { Path = "cds" }, new SingleRoute { Path = "alvs-cds" }] };
        var stubUrl = routeConfig.StubUrl.TrimEnd('/');
        _routes = routeConfig.Routes
            .Select(x => new
            {
                Path = x.Path.Trim('/'), 
                Url = x.SelectedRoute switch
                {
                    SelectedRoute.New => x.NewUrl,
                    SelectedRoute.Legacy => x.LegacyUrl,
                    _ => null
                } ?? stubUrl
            })
            .Select(x => x with { Url = $"{x.Url.TrimEnd('/')}/{x.Path.TrimStart('/')}" })
            .Concat([new { Path = TestPath, Url = $"{stubUrl}/{TestPath}" }])
            .ToDictionary(x => x.Path.ToLower(), x => x.Url.ToLower());
    }

    public string? GetRoute(string from)
    {
        return _routes.TryGetValue(from.Trim('/').ToLower(), out var value) ? value : null;
    }
}
