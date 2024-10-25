using System.Net;
using System.Net.Mime;
using System.Text;
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
        
        // Does this need to be written out both here and the Gateway?
        Console.WriteLine(request.HttpString());
        Console.WriteLine(messageBody);

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = MediaTypeNames.Application.Soap;
        var content = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(ResponseContent));
        await context.Response.BodyWriter.WriteAsync(content);
    }

    // Might need to make this target specific.
    private const string ResponseContent = """
<?xml version="1.0" encoding="utf-16" standalone="no"?>
<Envelope xmlns="http://www.w3.org/2003/05/soap-envelope/" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
    <Body>
        <Response xmlns="http://example.com/"/>
    </Body>
</Envelope>
""";
}
