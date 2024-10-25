using CdmsGateway.Stub.Utils;

namespace CdmsGateway.Stub.Services;

public class StubMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        request.EnableBuffering();
        var messageBody = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;
        
        Console.WriteLine(request.HttpString());
        Console.WriteLine(messageBody);
    }
}
