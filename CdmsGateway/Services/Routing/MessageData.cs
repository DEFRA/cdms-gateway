using System.Net.Mime;
using System.Text;
using System.Text.Json.Nodes;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services.Routing;

public class MessageData
{
    private readonly ILogger _logger;
    private const string CorrelationIdName = "correlation-id";
    private const string CorrelationIdSoapName = "X-Correlation-ID";

    public string CorrelationId { get; }
    public string ContentAsString { get; }
    public string HttpString { get; }
    public string Path { get; }

    private readonly string _method;
    private readonly string _contentType;
    private readonly IHeaderDictionary _headers;

    public static async Task<MessageData> Create(HttpRequest request, ILogger logger)
    {
        var content = await RetrieveContent(request);
        return new MessageData(request, content, logger);
    }

    private MessageData(HttpRequest request, string contentAsString, ILogger logger)
    {
        _logger = logger;
        try
        {
            ContentAsString = contentAsString;
            _method = request.Method;
            Path = request.Path.HasValue ? request.Path.Value.Trim('/') : string.Empty;
            _contentType = RetrieveContentType(request);
            _headers = request.Headers;
            HttpString = $"{request.Protocol} {_method} {request.Scheme}://{request.Host}{request.Path}{request.QueryString} {_contentType}";       
            CorrelationId = _headers[CorrelationIdSoapName].FirstOrDefault() ?? Guid.NewGuid().ToString("D");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error constructing message data");
            throw;
        }
    }

    public bool ShouldProcessRequest() => !(_method == HttpMethods.Get && Path == "health");

    public HttpRequestMessage CreateForwardingRequest(string? routeUrl)
    {
        try
        {
            var request = new HttpRequestMessage(new HttpMethod(_method), routeUrl);
            foreach (var header in _headers.Where(x => !x.Key.StartsWith("Content-"))) 
                request.Headers.Add(header.Key, header.Value.ToArray());
            request.Headers.Add(CorrelationIdName, CorrelationId);
        
            request.Content = _contentType == MediaTypeNames.Application.Json 
                ? JsonContent.Create(JsonNode.Parse(string.IsNullOrWhiteSpace(ContentAsString) ? "{}" : ContentAsString)) 
                : new StringContent(ContentAsString, Encoding.UTF8, _contentType);

            return request;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating forwarding request");
            throw;
        }
    }

    public async Task PopulateResponse(HttpResponse response, RoutingResult routingResult)
    {
        try
        {
            response.StatusCode = (int)routingResult.StatusCode;
            response.ContentType = _contentType;
            response.Headers.Date = (routingResult.ResponseDate ?? DateTimeOffset.Now).ToString("R");
            response.Headers[CorrelationIdSoapName] = CorrelationId;
            if (routingResult.ResponseContent != null)
                await response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(routingResult.ResponseContent)));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error populating response");
            throw;
        }
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