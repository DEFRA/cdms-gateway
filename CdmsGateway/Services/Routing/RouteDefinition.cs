namespace CdmsGateway.Services.Routing;

public class RouteDefinition(string? url)
{
    public static RouteDefinition Empty => new(null) { IsEmpty = true };

    public string Url => $"{url}{Path}";
    public string Path { get; init; } = "";
    public string MediaType { get; init; } = "";
    public bool IsEmpty { get; private init; }
}