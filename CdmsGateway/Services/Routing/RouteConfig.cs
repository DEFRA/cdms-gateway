namespace CdmsGateway.Services.Routing;

public record RouteConfig
{
    public required string StubUrl { get; init; }
    public required SingleRoute[] Routes { get; init; } = [];
}

public record SingleRoute
{
    public required string Name { get; init; }
    public SelectedRoute? SelectedRoute { get; set; }
    public string? LegacyUrl { get; init; }
    public string? NewUrl { get; init; }
}

public enum SelectedRoute { Stub, Legacy, New };