namespace CdmsGateway.Stub.Utils;

public static class Extensions
{
    public static string HttpString(this HttpRequest request) => $"{request.Method} {request.Scheme}//{request.Host}{request.Path}{request.QueryString} {request.Protocol} {request.ContentType}";
}