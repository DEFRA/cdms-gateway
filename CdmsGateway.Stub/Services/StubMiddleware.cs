namespace CdmsGateway.Stub.Services;

public class StubMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        request.EnableBuffering();
        var messageBody = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;
        
        Console.WriteLine($"{request.Method} {request.Scheme}//{request.Host}{request.Path} {request.Protocol} {request.ContentType}");
        Console.WriteLine(messageBody);
    }
}
