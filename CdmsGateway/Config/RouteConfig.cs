namespace CdmsGateway.Config;

public record RouteConfig
{
    public required string StubUrl { get; init; }
    public required SingleRoute[] RealRoutes { get; init; } = [];
}

public record SingleRoute
{
    public required string Path { get; init; }
    public required string? Url { get; init; }
}