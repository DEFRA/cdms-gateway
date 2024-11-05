// using System.Net;
// using System.Net.Mime;
// using System.Text;
// using CdmsGateway.Services.Routing;
// using CdmsGateway.Test.Utils;
// using FluentAssertions;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace CdmsGateway.Test;
//
// public class GatewayEndToEndTests : IAsyncDisposable
// {
//     private const string XmlContent = "<xml>Content</xml>";
//
//     private readonly string _headerCorrelationId = Guid.NewGuid().ToString("D");
//     private readonly DateTimeOffset _headerDate = DateTimeOffset.UtcNow.AddSeconds(-1).RoundDownToSecond();
//     private readonly TestWebServer _testWebServer = TestWebServer.BuildAndRun();
//     private readonly HttpClient _httpClient;
//
//     public GatewayEndToEndTests()
//     {
//         _httpClient = _testWebServer.HttpClient;
//         _httpClient.DefaultRequestHeaders.Date = _headerDate;
//         _httpClient.DefaultRequestHeaders.Add(TestHttpHandler.CorrelationIdHeaderName, _headerCorrelationId);
//     }
//
//     public async ValueTask DisposeAsync() => await _testWebServer.DisposeAsync();
//
//     [Fact]
//     public async Task When_checking_service_health_Should_be_healthy()
//     {
//         var response = await _httpClient.GetAsync("health");
//         
//         response.StatusCode.Should().Be(HttpStatusCode.OK);
//         response.Content.Headers.ContentType?.ToString().Should().Be(MediaTypeNames.Text.Plain);
//         (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
//     }
//
//     [Fact]
//     public async Task When_routing_request_Should_respond_correctly()
//     {
//         var response = await _httpClient.PostAsync("alvs-ipaffs/sub-path", new StringContent(XmlContent, Encoding.UTF8, MediaTypeNames.Application.Xml));
//         
//         response.StatusCode.Should().Be(HttpStatusCode.OK);
//         response.Content.Headers.ContentType?.ToString().Should().Be(MediaTypeNames.Application.Xml);
//         response.Headers.Date.Should().BeAfter(_headerDate);
//         response.Headers.GetValues(TestHttpHandler.CorrelationIdHeaderName).FirstOrDefault().Should().Be(_headerCorrelationId);
//         (await response.Content.ReadAsStringAsync()).Should().Be(TestHttpHandler.XmlRoutedResponse);
//     }
//
//     [Fact]
//     public async Task When_routing_request_Should_route_correctly()
//     {
//         var expectedRoutUrl = _testWebServer.Services.GetRequiredService<RouteConfig>().StubUrl; 
//         _testWebServer.TestHttpHandler.ExpectRouteUrl($"{expectedRoutUrl}alvs-ipaffs/sub-path")
//                                       .ExpectRouteMethod("POST")
//                                       .ExpectRouteHeaderDate(_headerDate)
//                                       .ExpectRouteHeaderCorrelationId(_headerCorrelationId)
//                                       .ExpectRouteContentType(MediaTypeNames.Application.Xml)
//                                       .ExpectRouteContent(XmlContent);
//
//         await _httpClient.PostAsync("alvs-ipaffs/sub-path", new StringContent(XmlContent, Encoding.UTF8, MediaTypeNames.Application.Xml));
//
//         _testWebServer.TestHttpHandler.WasExpectedRequestSent().Should().BeTrue();
//         _testWebServer.TestHttpHandler.Response?.StatusCode.Should().Be(HttpStatusCode.OK);
//         _testWebServer.TestHttpHandler.Response?.Headers.Date.Should().BeAfter(_headerDate);
//         _testWebServer.TestHttpHandler.Response?.Headers.GetValues(TestHttpHandler.CorrelationIdHeaderName).FirstOrDefault().Should().Be(_headerCorrelationId);
//         _testWebServer.TestHttpHandler.Response?.Content.Headers.ContentType?.ToString().Should().StartWith(MediaTypeNames.Application.Xml);
//         (await _testWebServer.TestHttpHandler.Response?.Content.ReadAsStringAsync()!).Should().Be(TestHttpHandler.XmlRoutedResponse);
//     }
// }