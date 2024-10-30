using System.Net;
using System.Net.Mime;
using System.Text;
using CdmsGateway.Services.Routing;
using CdmsGateway.Test.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdmsGateway.Test;

public class GatewayEndToEndTests : IAsyncDisposable
{
    private const string XmlContent = "<xml>Content</xml>";
    private readonly TestWebServer _testWebServer = TestWebServer.BuildAndRun();

    public async ValueTask DisposeAsync() => await _testWebServer.DisposeAsync();

    [Fact]
    public async Task When_checking_service_health_Should_be_healthy()
    {
        var response = await _testWebServer.HttpClient.GetAsync("health");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.ToString().Should().Be(MediaTypeNames.Text.Plain);
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }

    [Fact]
    public async Task When_routing_request_Should_respond_correctly()
    {
        var response = await _testWebServer.HttpClient.PostAsync("alvs-apaffs", new StringContent(XmlContent, Encoding.UTF8, MediaTypeNames.Application.Soap));
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.ToString().Should().Be(MediaTypeNames.Application.Soap);
        (await response.Content.ReadAsStringAsync()).Should().Be(TestHttpHandler.DefaultXmlRoutedResponse);
    }

    [Fact]
    public async Task When_routing_request_Should_route_correctly()
    {
        var expectedRoutUrl = _testWebServer.Services.GetRequiredService<RouteConfig>().StubUrl; 
        _testWebServer.TestHttpHandler.ExpectRouteUrl($"{expectedRoutUrl}alvs-apaffs")
                                      .ExpectRouteMethod("POST")
                                      .ExpectRouteContentType(MediaTypeNames.Application.Soap)
                                      .ExpectRouteContent(XmlContent);

        await _testWebServer.HttpClient.PostAsync("alvs-apaffs", new StringContent(XmlContent, Encoding.UTF8, MediaTypeNames.Application.Soap));

        _testWebServer.TestHttpHandler.WasExpectedRequestSent().Should().BeTrue();
        _testWebServer.TestHttpHandler.RoutedResponseStatusCode.Should().Be(HttpStatusCode.OK);
        _testWebServer.TestHttpHandler.RoutedResponseContentType.Should().Be(MediaTypeNames.Application.Soap);
        _testWebServer.TestHttpHandler.RoutedResponseContent.Should().NotBeNull();
    }
}