using CdmsGateway.Config;
using CdmsGateway.Services.Routing;
using FluentAssertions;

namespace CdmsGateway.Test.Services.Routing;

public class MessageRoutesTests
{
    [Theory]
    [InlineData(SelectedRoute.Stub, "/alvs-ipaffs/sub-path/", "alvs-ipaffs", "http://stub/alvs-ipaffs/sub-path")]
    [InlineData(SelectedRoute.Legacy, "/alvs-ipaffs/sub-path/", "alvs-ipaffs", "http://legacy-alvs-ipaffs/somewhere/sub-path")]
    [InlineData(SelectedRoute.New, "/alvs-ipaffs/sub-path/", "alvs-ipaffs", "http://new-alvs-ipaffs/somewhere/sub-path")]
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
    [InlineData(SelectedRoute.Stub, "/alvs-ipaffs/sub-path/", "alvs-ipaffs", "http://stub/alvs-ipaffs/sub-path")]
    [InlineData(SelectedRoute.Legacy, "/alvs-ipaffs/sub-path/", "alvs-ipaffs", "http://stub/alvs-ipaffs/sub-path")]
    [InlineData(SelectedRoute.New, "/alvs-ipaffs/sub-path/", "alvs-ipaffs", "http://stub/alvs-ipaffs/sub-path")]
    [InlineData(null, "/alvs-ipaffs/sub-path/", "alvs-ipaffs", "http://stub/alvs-ipaffs/sub-path")]
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
                Name = "alvs-ipaffs",
                SelectedRoute = selectedRoute,
                LegacyUrl = "http://legacy-alvs-ipaffs/somewhere/",
                NewUrl = "http://new-alvs-ipaffs/somewhere/"
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
                Name = "alvs-ipaffs",
                SelectedRoute = selectedRoute
            }
        ]
    });
}