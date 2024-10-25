using System.Net.Mime;

namespace CdmsGateway.Services.Routing;

public interface IMessageRoutes
{
    RouteDefinition GetRoute(string from);
}

public class MessageRoutes : IMessageRoutes
{
    // Should get from config with alternate real routes
    private const string StubUrl = "http://localhost:3092/";

    private static readonly Dictionary<string, RouteDefinition> StubRoutes = new()
    {
        { "alvs-apaffs", new RouteDefinition(StubUrl) { Path = "alvs-apaffs-stub", MediaType = MediaTypeNames.Application.Xml } },
        { "cds", new RouteDefinition(StubUrl) { Path = "cds-stub", MediaType = MediaTypeNames.Application.Xml } },
        { "alvs-cds", new RouteDefinition(StubUrl) { Path = "alvs-cds-stub", MediaType = MediaTypeNames.Application.Xml } },
    };

    public RouteDefinition GetRoute(string from)
    {
        return StubRoutes.TryGetValue(from.ToLower().Trim('/'), out var value) ? value : RouteDefinition.Empty;
    }
}