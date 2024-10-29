using System.Net;
using System.Net.Mime;
using System.Text;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace CdmsGateway.Test.Utils;

public class TestHttpHandler : DelegatingHandler
{
    public const string DefaultXmlRoutedResponse = "<xml>RoutedResponse</xml>";

    public TestHttpHandler ExpectRouteUrl(string routeUrl) { _routeUrl = routeUrl; return this; }
    public TestHttpHandler ExpectRouteMethod(string routeMethod) { _routeMethod = routeMethod; return this; }
    public TestHttpHandler ExpectRouteContentType(string routeContentType) { _routeContentType = routeContentType; return this; }
    public TestHttpHandler ExpectRouteContent(string routeContent) { _routeContent = routeContent; return this; }

    public HttpStatusCode RoutedResponseStatusCode = HttpStatusCode.OK;
    public string RoutedResponseContentType = MediaTypeNames.Application.Soap;
    public string RoutedResponseContent = DefaultXmlRoutedResponse;

    public HttpRequestMessage? Request { get; private set; }
    
    private string? _routeUrl;
    private string? _routeMethod;
    private string? _routeContentType;
    private string? _routeContent;
    private string? _routedContent;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Request = request;

        _routedContent = await request.Content?.ReadAsStringAsync(cancellationToken)!;

        return new HttpResponseMessage(RoutedResponseStatusCode)
        {
            Content = new StringContent(RoutedResponseContent, Encoding.UTF8, RoutedResponseContentType)
        };
    }

    public bool WasExpectedRequestSent() => Request != null && 
                                            Request.RequestUri?.ToString() == _routeUrl &&
                                            Request.Method.ToString() == _routeMethod &&
                                            Request.Content?.Headers.ContentType?.ToString().StartsWith(_routeContentType!) == true &&
                                            _routedContent == _routeContent;
}