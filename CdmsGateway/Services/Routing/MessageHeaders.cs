using System.Net.Mime;

namespace CdmsGateway.Services.Routing;

public class MessageHeaders
{
    public const string CorrelationIdName = "X-Correlation-ID";

    public MessageHeaders(HttpRequest request)
    {
        var contentTypeParts = request.ContentType?.Split(';');
        ContentType = contentTypeParts is { Length: > 0 } ? contentTypeParts[0] : MediaTypeNames.Application.Json;
        Authorization = request.Headers.Authorization.FirstOrDefault();
        Date = request.Headers.Date.FirstOrDefault();
        CorrelationId = request.Headers[CorrelationIdName].FirstOrDefault() ?? Guid.NewGuid().ToString("D");
    }
    
    public string ContentType { get; init; }
    public string? Authorization { get; init; }
    public string? Date { get; init; }
    public string CorrelationId { get; init; }
}