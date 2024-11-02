using System.Net.Mime;
using System.Text;
using System.Text.Json.Nodes;

namespace CdmsGateway.Services.Routing;

public class MessageData
{
    private const string CorrelationIdName = "correlation-id";
    private const string CorrelationIdSoapName = "X-Correlation-ID";

    public string CorrelationId { get; }
    public string ContentAsString { get; }
    public string HttpString { get; }
    public string Path { get; }

    private readonly string _method;
    private readonly string _contentType;
    private readonly IHeaderDictionary _headers;

    public static async Task<MessageData> Create(HttpRequest request)
    {
        var content = await RetrieveContent(request);
        return new MessageData(request, content);
    }

    private MessageData(HttpRequest request, string contentAsString)
    {
        ContentAsString = contentAsString;
        _method = request.Method;
        Path = request.Path.HasValue ? request.Path.Value.Trim('/').ToLower() : string.Empty;
        _contentType = RetrieveContentType(request);
        _headers = request.Headers;
        HttpString = $"{_method} {request.Scheme}://{request.Host}{request.Path}{request.QueryString} {request.Protocol} {_contentType}";       
        CorrelationId = _headers[CorrelationIdSoapName].FirstOrDefault() ?? Guid.NewGuid().ToString("D");
    }

    public bool ShouldProcessRequest() => !(_method == HttpMethods.Get && Path == "health");

    public HttpRequestMessage CreateForwardingRequest(string? routeUrl)
    {
        var request = new HttpRequestMessage(new HttpMethod(_method), routeUrl);
        foreach (var header in _headers.Where(x => !x.Key.StartsWith("Content-"))) 
            request.Headers.Add(header.Key, header.Value.ToArray());
        request.Headers.Add(CorrelationIdName, CorrelationId);
        
        request.Content = _contentType == MediaTypeNames.Application.Json 
            ? JsonContent.Create(JsonNode.Parse(ContentAsString)) 
            : new StringContent(ContentAsString, Encoding.UTF8, _contentType);

        return request;
    }

    public async Task PopulateResponse(HttpResponse response, RoutingResult routingResult)
    {
        response.StatusCode = (int)routingResult.StatusCode;
        response.ContentType = _contentType;
        response.Headers.Date = (routingResult.ResponseDate ?? DateTimeOffset.Now).ToString("R");
        response.Headers[CorrelationIdSoapName] = CorrelationId;
        if (routingResult.ResponseContent != null)
            await response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(routingResult.ResponseContent)));
    }

    private static async Task<string> RetrieveContent(HttpRequest request)
    {
        request.EnableBuffering();
        var content = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;
        return content;
    }

    private string RetrieveContentType(HttpRequest request)
    {
        var contentTypeParts = request.ContentType?.Split(';');
        return contentTypeParts is { Length: > 0 } ? contentTypeParts[0] : MediaTypeNames.Application.Json;
    }
}