using CdmsGateway.Config;
using CdmsGateway.Services.Routing;
using FluentAssertions;

namespace CdmsGateway.Test.Services.Routing;

public class MessageRoutesTests
{
    [Theory]
    [InlineData(SelectedRoute.Stub, "alvs-apaffs", "http://stub/alvs-apaffs")]
    [InlineData(SelectedRoute.Legacy, "alvs-apaffs", "http://legacy-alvs-apaffs/alvs-apaffs")]
    [InlineData(SelectedRoute.New, "alvs-apaffs", "http://new-alvs-apaffs/alvs-apaffs")]
    [InlineData(SelectedRoute.Stub, "cds", "http://stub/cds")]
    [InlineData(SelectedRoute.Legacy, "cds", "http://legacy-cds/cds")]
    [InlineData(SelectedRoute.New, "cds", "http://new-cds/cds")]
    [InlineData(SelectedRoute.Stub, "alvs-cds", "http://stub/alvs-cds")]
    [InlineData(SelectedRoute.Legacy, "alvs-cds", "http://legacy-alvs-cds/alvs-cds")]
    [InlineData(SelectedRoute.New, "alvs-cds", "http://new-alvs-cds/alvs-cds")]
    [InlineData(SelectedRoute.Stub, "test", "http://stub/test")]
    [InlineData(SelectedRoute.Legacy, "test", "http://stub/test")]
    [InlineData(SelectedRoute.New, "test", "http://stub/test")]
    [InlineData(null, "test", "http://stub/test")]
    [InlineData(null, "xyz", null)]
    public void When_routing_through_a_fully_specified_routing_table_Should_reach_correct_route(SelectedRoute? selectedRoute, string routedPath, string? expectedRoute)
    {
        var messageRoutes = GetFullyRoutedMessageRoutes(selectedRoute);

        messageRoutes.GetRoute(routedPath).Should().Be(expectedRoute);
    }

    [Theory]
    [InlineData(SelectedRoute.Stub, "alvs-apaffs", "http://stub/alvs-apaffs")]
    [InlineData(SelectedRoute.Legacy, "alvs-apaffs", "http://stub/alvs-apaffs")]
    [InlineData(SelectedRoute.New, "alvs-apaffs", "http://stub/alvs-apaffs")]
    [InlineData(null, "alvs-apaffs", "http://stub/alvs-apaffs")]
    public void When_routing_through_a_routing_table_without_legacy_or_new_routes_Should_reach_stub_route(SelectedRoute? selectedRoute, string routedPath, string? expectedRoute)
    {
        var messageRoutes = GetNullRoutedMessageRoutes(selectedRoute);

        messageRoutes.GetRoute(routedPath).Should().Be(expectedRoute);
    }

    private static MessageRoutes GetFullyRoutedMessageRoutes(SelectedRoute? selectedRoute) => new(new RouteConfig
    {
        StubUrl = "http://stub/",
        Routes = [
            new SingleRoute {
                Path = "alvs-apaffs",
                SelectedRoute = selectedRoute,
                LegacyUrl = "http://legacy-alvs-apaffs/",
                NewUrl = "http://new-alvs-apaffs/"
            },
            new SingleRoute {
                Path = "cds",
                SelectedRoute = selectedRoute,
                LegacyUrl = "http://legacy-cds/",
                NewUrl = "http://new-cds/"
            },
            new SingleRoute {
                Path = "alvs-cds",
                SelectedRoute = selectedRoute,
                LegacyUrl = "http://legacy-alvs-cds/",
                NewUrl = "http://new-alvs-cds/"
            }
        ]
    });

    private static MessageRoutes GetNullRoutedMessageRoutes(SelectedRoute? selectedRoute) => new(new RouteConfig
    {
        StubUrl = "http://stub/",
        Routes = [
            new SingleRoute {
                Path = "alvs-apaffs",
                SelectedRoute = selectedRoute
            }
        ]
    });
}