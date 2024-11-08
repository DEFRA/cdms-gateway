using System.Net;
using System.Text;
using CdmsGateway.Services;

namespace CdmsGateway.Test.Utils;

public class TestHttpHandler : DelegatingHandler
{
    public const string XmlRoutedResponse = "<xml>RoutedResponse</xml>";

    public HttpRequestMessage? Request;
    public HttpResponseMessage? Response;

    private Func<HttpStatusCode> _responseStatusCodeFunc = () => HttpStatusCode.OK;

    public void ShouldErrorWithStatus(Func<HttpStatusCode> statusCodeFunc) => _responseStatusCodeFunc = statusCodeFunc;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var responseStatusCode = _responseStatusCodeFunc();
        if (responseStatusCode != HttpStatusCode.OK) return Task.FromResult(new HttpResponseMessage(responseStatusCode));

        Request = request;

        Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(XmlRoutedResponse, Encoding.UTF8, request.Content?.Headers.ContentType!)
        };
        Response.Headers.Date = DateTimeOffset.UtcNow;
        Response.Headers.Add(MessageData.CorrelationIdName, request.Headers.GetValues(MessageData.CorrelationIdName));

        return Task.FromResult(Response);
    }
}