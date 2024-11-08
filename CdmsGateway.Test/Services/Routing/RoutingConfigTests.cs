using FluentAssertions;

namespace CdmsGateway.Test.Services.Routing;

public class RoutingConfigTests
{
    [Theory]
    [InlineData("route-1", "http://url-1/")]
    [InlineData("route-2", "http://url-2/")]
    [InlineData("route-3", "http://url-3/")]
    [InlineData("route-4", "http://url-1/")]
    public void When_getting_returned_routes_Then_should_retrieve_returned_urls(string routeName, string url)
    {
        TestRoutes.RoutingConfig.AllReturnedRoutes.Single(x => x.Name == routeName).Url.Should().Be(url);
    }
    
    [Theory]
    [InlineData("route-1", "http://url-2/")]
    [InlineData("route-2", "http://url-3/")]
    [InlineData("route-3", "http://url-4/")]
    [InlineData("route-4", "http://url-3/")]
    public void When_getting_unreturned_routes_Then_should_retrieve_unreturned_urls(string routeName, string url)
    {
        TestRoutes.RoutingConfig.AllUnreturnedRoutes.Single(x => x.Name == routeName).Url.Should().Be(url);
    }
}