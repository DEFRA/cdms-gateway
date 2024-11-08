namespace CdmsGateway.Services.Routing;

public record RoutingConfig
{
    public RoutedUrl[] AllReturnedRoutes => Routes.Concat(AdditionalRoutes).Join(NamedUrls, nr => nr.RoutedUrlName, nu => nu.Name, (nr, nu) => new RoutedUrl { Name = nr.Name, Url = nu.Url }).ToArray();
    public RoutedUrl[] AllUnreturnedRoutes => Routes.Concat(AdditionalRoutes).Join(NamedUrls, nr => nr.UnreturnedUrlName, nu => nu.Name, (nr, nu) => new RoutedUrl { Name = nr.Name, Url = nu.Url }).ToArray();
    
    public required NamedRoute[] Routes { get; init; } = [];
    public required NamedRoute[] AdditionalRoutes { get; init; } = [];
    public required NamedUrl[] NamedUrls { get; init; } = [];
}

public record NamedRoute
{
    public required string Name { get; init; }
    public required string RoutedUrlName { get; init; }
    public required string UnreturnedUrlName { get; init; }
}

public record NamedUrl
{
    public required string Name { get; init; }
    public required string Url { get; init; }
}

public record RoutedUrl
{
    public required string Name { get; init; }
    public required string Url { get; init; }
}