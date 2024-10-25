using System.Net.Mime;
using System.Text;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils;

namespace CdmsGateway.Services;

public class SoapInterceptorMiddleware(RequestDelegate next, IMessageRouter messageRouter)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        if (request.Method == HttpMethods.Post && request.Path.HasValue)
        {
            var messageBody = await RetrieveMessageBody(request);

            // Does this need to be written out both here and the Stub?
            Console.WriteLine(request.HttpString());
            Console.WriteLine(messageBody);

            var routingResult = await messageRouter.Route(request.Path, messageBody);

            if (routingResult.RouteFound)
            {
                if (routingResult.RoutedSuccessfully)
                {
                    Console.WriteLine(routingResult.ResponseContent);
                    Console.WriteLine($"Successfully routed to {routingResult.RouteUrl}");
                }
                else
                {
                    Console.WriteLine($"Failed to route to {routingResult.RouteUrl} with response code {routingResult.StatusCode}");
                }

                await CreateResponse(context, routingResult);

                return;
            }
        }

        Console.WriteLine("Routing failed");

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
