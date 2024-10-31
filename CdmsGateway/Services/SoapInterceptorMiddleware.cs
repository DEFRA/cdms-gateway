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
            var request = context.Request;
            var messageHeaders = new MessageHeaders(request);
            if (request.Method == HttpMethods.Post && request.Path.HasValue)
            {
                var messageBody = await RetrieveMessageBody(request);
                var correlationId = messageHeaders.CorrelationId;

                logger.Information("{CorrelationId} {HttpString} {MessageBody}", correlationId, request.HttpString(), messageBody);

                var routingResult = await messageRouter.Route(request.Path, messageBody, messageHeaders);

                if (routingResult.RouteFound)
                {
                    if (routingResult.RoutedSuccessfully)
                    {
                        logger.Information("{CorrelationId} Successfully routed to {RouteUrl} with content {Content}", correlationId, routingResult.RouteUrl, routingResult.ResponseContent);
                    }
                    else
                    {
                        logger.Information("{CorrelationId} Failed to route to {RouteUrl} with status code {StatusCode}", correlationId, routingResult.RouteUrl, routingResult.StatusCode);
                    }

                    await CreateResponse(context.Response, routingResult, messageHeaders);

                    return;
                }

                logger.Information("{CorrelationId} Routing not supported for [{HttpString}]", correlationId, request.HttpString());
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Routing threw an exception");
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

    private static async Task CreateResponse(HttpResponse response, RoutingResult routingResult, MessageHeaders messageHeaders)
    {
        response.StatusCode = (int)routingResult.StatusCode;
        response.ContentType = messageHeaders.ContentType;
        response.Headers.Authorization = messageHeaders.Authorization;
        response.Headers.Date = messageHeaders.Date;
        response.Headers[MessageHeaders.CorrelationIdName] = messageHeaders.CorrelationId;
        if (routingResult.ResponseContent != null)
            await response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(routingResult.ResponseContent)));
    }
}