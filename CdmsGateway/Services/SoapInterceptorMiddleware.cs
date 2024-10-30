using System.Net.Mime;
using System.Text;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils;

namespace CdmsGateway.Services;

public class SoapInterceptorMiddleware(RequestDelegate next, IMessageRouter messageRouter)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        var request = context.Request;
        if (request.Method == HttpMethods.Post && request.Path.HasValue)
        {
            var messageBody = await RetrieveMessageBody(request);

            // Could this be written in the Stub?
            Console.WriteLine($"{correlationId} {request.HttpString()}");
            Console.WriteLine($"{correlationId} {messageBody}");

            var routingResult = await messageRouter.Route(request.Path, messageBody, correlationId);

            if (routingResult.RouteFound)
            {
                if (routingResult.RoutedSuccessfully)
                {
                    Console.WriteLine($"{correlationId} {routingResult.ResponseContent}");
                    Console.WriteLine($"{correlationId} Successfully routed to {routingResult.RouteUrl}");
                }
                else
                {
                    Console.WriteLine($"{correlationId} Failed to route to {routingResult.RouteUrl} with response code {routingResult.StatusCode}");
                }

                await CreateResponse(context, routingResult);

                return;
            }
        }

        Console.WriteLine($"{correlationId} Routing not supported for [{request.HttpString()}]");

        await next(context);
    }

    private static async Task<string> RetrieveMessageBody(HttpRequest request)
    {
        request.EnableBuffering();
        var messageBody = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;
        return messageBody;
    }

    private static async Task CreateResponse(HttpContext context, RoutingResult routingResult)
    {
        context.Response.StatusCode = (int)routingResult.StatusCode;
        context.Response.ContentType = MediaTypeNames.Application.Soap;
        if (routingResult.ResponseContent != null)
            await context.Response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(routingResult.ResponseContent)));
    }
}
