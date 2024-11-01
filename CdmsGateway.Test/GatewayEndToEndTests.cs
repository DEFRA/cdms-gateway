using System.Net;
using System.Net.Http.Headers;
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
    private const string HeaderAuthorization = "Bearer";
    private const string HeaderDate = "Sun, 03 Nov 2024 08:49:37 GMT";
    private static readonly string HeaderCorrelationId = Guid.NewGuid().ToString("D");

    private readonly TestWebServer _testWebServer = TestWebServer.BuildAndRun();
    private readonly HttpClient _httpClient;

    public GatewayEndToEndTests()
    {
        _httpClient = _testWebServer.HttpClient;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(HeaderAuthorization);
        _httpClient.DefaultRequestHeaders.Add("Date", HeaderDate);
        _httpClient.DefaultRequestHeaders.Add(TestHttpHandler.CorrelationIdHeaderName, HeaderCorrelationId);
    }

    public async ValueTask DisposeAsync() => await _testWebServer.DisposeAsync();

    [Fact]
    public async Task When_checking_service_health_Should_be_healthy()
    {
        var response = await _httpClient.GetAsync("health");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.ToString().Should().Be(MediaTypeNames.Text.Plain);
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }

    [Fact]
    public async Task When_routing_request_Should_respond_correctly()
    {
        var response = await _httpClient.PostAsync("alvs-apaffs/sub-path", new StringContent(XmlContent, Encoding.UTF8, MediaTypeNames.Application.Xml));
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.ToString().Should().Be(MediaTypeNames.Application.Xml);
        response.Headers.GetValues("Date").FirstOrDefault().Should().Be(HeaderDate);
        response.Headers.GetValues(TestHttpHandler.CorrelationIdHeaderName).FirstOrDefault().Should().Be(HeaderCorrelationId);
        (await response.Content.ReadAsStringAsync()).Should().Be(TestHttpHandler.XmlRoutedResponse);
    }

    [Fact]
    public async Task When_routing_request_Should_route_correctly()
    {
        var expectedRoutUrl = _testWebServer.Services.GetRequiredService<RouteConfig>().StubUrl; 
        _testWebServer.TestHttpHandler.ExpectRouteUrl($"{expectedRoutUrl}alvs-apaffs/sub-path")
                                      .ExpectRouteMethod("POST")
                                      .ExpectRouteAuthorization(HeaderAuthorization)
                                      .ExpectRouteHeaderDate(HeaderDate)
                                      .ExpectRouteHeaderCorrelationId(HeaderCorrelationId)
                                      .ExpectRouteContentType(MediaTypeNames.Application.Xml)
                                      .ExpectRouteContent(XmlContent);

        await _httpClient.PostAsync("alvs-apaffs/sub-path", new StringContent(XmlContent, Encoding.UTF8, MediaTypeNames.Application.Xml));

        _testWebServer.TestHttpHandler.WasExpectedRequestSent().Should().BeTrue();
        _testWebServer.TestHttpHandler.Response?.StatusCode.Should().Be(HttpStatusCode.OK);
        // _testWebServer.TestHttpHandler.Response?.Headers.GetValues("Date").FirstOrDefault().Should().Be(HeaderDate);
        _testWebServer.TestHttpHandler.Response?.Headers.GetValues(TestHttpHandler.CorrelationIdHeaderName).FirstOrDefault().Should().Be(HeaderCorrelationId);
        _testWebServer.TestHttpHandler.Response?.Content.Headers.ContentType?.ToString().Should().StartWith(MediaTypeNames.Application.Xml);
        (await _testWebServer.TestHttpHandler.Response?.Content.ReadAsStringAsync()!).Should().Be(TestHttpHandler.XmlRoutedResponse);
    }
}