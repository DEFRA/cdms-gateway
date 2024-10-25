namespace CdmsGateway.Utils;

public static class Extensions
{
    public static string HttpString(this HttpRequest request) => $"{request.Method} {request.Scheme}//{request.Host}{request.Path} {request.Protocol} {request.ContentType}";
}