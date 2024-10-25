namespace CdmsGateway.Services.Routing;

public record RouteDefinition
{
    public static RouteDefinition Empty => new() { IsEmpty = true };

    public string Url { get; init; } = "";
    public string Path { get; init; } = "";
    public string ContentType { get; init; } = "";
    public bool IsEmpty { get; private init; }
}
