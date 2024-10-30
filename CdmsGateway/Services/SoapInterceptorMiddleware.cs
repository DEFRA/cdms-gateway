using System.Net.Mime;
using System.Text;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services;

public class SoapInterceptorMiddleware(RequestDelegate next, IMessageRouter messageRouter, ILogger logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var correlationId = Guid.NewGuid().ToString("N");
            var request = context.Request;
            if (request.Method == HttpMethods.Post && request.Path.HasValue)
            {
                var messageBody = await RetrieveMessageBody(request);

                // Could this be written in the Stub?
                Console.WriteLine($"{correlationId} {request.HttpString()}");
                Console.WriteLine($"{correlationId} {messageBody}");
                logger.Information("{CorrelationId} {HttpString}", correlationId, request.HttpString());
                logger.Information("{CorrelationId} {MessageBody}", correlationId, messageBody);

                var routingResult = await messageRouter.Route(request.Path, messageBody, correlationId);
            
                if (routingResult.RouteFound)
                {
                    if (routingResult.RoutedSuccessfully)
                    {
                        Console.WriteLine($"{correlationId} {routingResult.ResponseContent}");
                        Console.WriteLine($"{correlationId} Successfully routed to {routingResult.RouteUrl}");
                        logger.Information("{CorrelationId} {RoutingResultResponseContent}", correlationId, routingResult.ResponseContent);
                        logger.Information("{CorrelationId} Successfully routed to {RoutingResultRouteUrl}", correlationId, routingResult.RouteUrl);
                    }
                    else
                    {
                        Console.WriteLine($"{correlationId} Failed to route to {routingResult.RouteUrl} with response code {routingResult.StatusCode}");
                        logger.Information("{CorrelationId} Failed to route to {RoutingResultRouteUrl} with response code {RoutingResultStatusCode}", correlationId, routingResult.RouteUrl, routingResult.StatusCode);
                    }
            
                    await CreateResponse(context, routingResult);
            
                    return;
                }
            }

            Console.WriteLine($"{correlationId} Routing not supported for [{request.HttpString()}]");
            logger.Information("{CorrelationId} Routing not supported for [{HttpString}]", correlationId, request.HttpString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Routing threw an exception {ex}");
            logger.Error(ex, "Routing threw an exception");
            throw;
        }

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
