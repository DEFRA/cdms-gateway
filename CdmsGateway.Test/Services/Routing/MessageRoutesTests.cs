using CdmsGateway.Config;
using CdmsGateway.Services.Routing;
using FluentAssertions;

namespace CdmsGateway.Test.Services.Routing;

public class MessageRoutesTests
{
    [Theory]
    [InlineData(SelectedRoute.Stub, "/alvs-apaffs/sub-path/", "alvs-apaffs", "http://stub/alvs-apaffs/sub-path")]
    [InlineData(SelectedRoute.Legacy, "/alvs-apaffs/sub-path/", "alvs-apaffs", "http://legacy-alvs-apaffs/somewhere/sub-path")]
    [InlineData(SelectedRoute.New, "/alvs-apaffs/sub-path/", "alvs-apaffs", "http://new-alvs-apaffs/somewhere/sub-path")]
    [InlineData(SelectedRoute.Stub, "/cds/sub-path/", "cds", "http://stub/cds/sub-path")]
    [InlineData(SelectedRoute.Legacy, "/cds/sub-path/", "cds", "http://legacy-cds/somewhere/sub-path")]
    [InlineData(SelectedRoute.New, "/cds/sub-path/", "cds", "http://new-cds/somewhere/sub-path")]
    [InlineData(SelectedRoute.Stub, "/alvs-cds/sub-path/", "alvs-cds", "http://stub/alvs-cds/sub-path")]
    [InlineData(SelectedRoute.Legacy, "/alvs-cds/sub-path/", "alvs-cds", "http://legacy-alvs-cds/somewhere/sub-path")]
    [InlineData(SelectedRoute.New, "/alvs-cds/sub-path/", "alvs-cds", "http://new-alvs-cds/somewhere/sub-path")]
    [InlineData(SelectedRoute.Stub, "/test/sub-path/", "test", "http://stub/test/sub-path")]
    [InlineData(SelectedRoute.Legacy, "/test/sub-path/", "test", "http://stub/test/sub-path")]
    [InlineData(SelectedRoute.New, "/test/sub-path/", "test", "http://stub/test/sub-path")]
    [InlineData(null, "/test/sub-path/", "test", "http://stub/test/sub-path")]
    [InlineData(null, "/xyz/sub-path/", "xyz", null)]
    public void When_routing_through_a_fully_specified_routing_table_Should_reach_correct_route(SelectedRoute? selectedRoute, string routedPath, string expectedRouteName, string? expectedRoutePath)
    {
        var messageRoutes = GetFullyRoutedMessageRoutes(selectedRoute);

        var route = messageRoutes.GetRoute(routedPath);
        
        route.RouteName.Should().Be(expectedRouteName);
        route.RouteUrl.Should().Be(expectedRoutePath);
    }

    [Theory]
    [InlineData(SelectedRoute.Stub, "/alvs-apaffs/sub-path/", "alvs-apaffs", "http://stub/alvs-apaffs/sub-path")]
    [InlineData(SelectedRoute.Legacy, "/alvs-apaffs/sub-path/", "alvs-apaffs", "http://stub/alvs-apaffs/sub-path")]
    [InlineData(SelectedRoute.New, "/alvs-apaffs/sub-path/", "alvs-apaffs", "http://stub/alvs-apaffs/sub-path")]
    [InlineData(null, "/alvs-apaffs/sub-path/", "alvs-apaffs", "http://stub/alvs-apaffs/sub-path")]
    public void When_routing_through_a_routing_table_without_legacy_or_new_routes_Should_reach_stub_route(SelectedRoute? selectedRoute, string routedPath, string expectedRouteName, string? expectedRoutePath)
    {
        var messageRoutes = GetNullRoutedMessageRoutes(selectedRoute);

        var route = messageRoutes.GetRoute(routedPath);

        route.RouteName.Should().Be(expectedRouteName);
        route.RouteUrl.Should().Be(expectedRoutePath);
    }

    private static MessageRoutes GetFullyRoutedMessageRoutes(SelectedRoute? selectedRoute) => new(new RouteConfig
    {
        StubUrl = "http://stub/",
        Routes = [
            new SingleRoute {
                Name = "alvs-apaffs",
                SelectedRoute = selectedRoute,
                LegacyUrl = "http://legacy-alvs-apaffs/somewhere/",
                NewUrl = "http://new-alvs-apaffs/somewhere/"
            },
            new SingleRoute {
                Name = "cds",
                SelectedRoute = selectedRoute,
                LegacyUrl = "http://legacy-cds/somewhere/",
                NewUrl = "http://new-cds/somewhere/"
            },
            new SingleRoute {
                Name = "alvs-cds",
                SelectedRoute = selectedRoute,
                LegacyUrl = "http://legacy-alvs-cds/somewhere/",
                NewUrl = "http://new-alvs-cds/somewhere/"
            }
        ]
    });

    private static MessageRoutes GetNullRoutedMessageRoutes(SelectedRoute? selectedRoute) => new(new RouteConfig
    {
        StubUrl = "http://stub/",
        Routes = [
            new SingleRoute {
                Name = "alvs-apaffs",
                SelectedRoute = selectedRoute
            }
        ]
    });
}