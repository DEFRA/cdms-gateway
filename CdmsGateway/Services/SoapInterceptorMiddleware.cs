using CdmsGateway.Services.Routing;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services;

public class SoapInterceptorMiddleware(RequestDelegate next, IMessageRouter messageRouter, ILogger logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var messageData = await MessageData.Create(context.Request, logger);
            if (messageData.ShouldProcessRequest())
            {
                logger.Information("{CorrelationId} {HttpString} {Content}", messageData.CorrelationId, messageData.HttpString, messageData.ContentAsString);

                var routingResult = await messageRouter.Route(messageData);

                if (routingResult.RouteFound)
                {
                    if (routingResult.RoutedSuccessfully)
                    {
                        logger.Information("{CorrelationId} Successfully routed to {RouteUrl} with response {StatusCode} {Content}", messageData.CorrelationId, routingResult.RouteUrl, routingResult.StatusCode, routingResult.ResponseContent);
                    }
                    else
                    {
                        logger.Information("{CorrelationId} Failed to route to {RouteUrl} with status code {StatusCode}", messageData.CorrelationId, routingResult.RouteUrl, routingResult.StatusCode);
                    }

                    await messageData.PopulateResponse(context.Response, routingResult);

                    return;
                }

                logger.Information("{CorrelationId} Routing not supported for [{HttpString}]", messageData.CorrelationId, messageData.HttpString);
            }
            
            logger.Information("{CorrelationId} Pass through request {HttpString}", messageData.CorrelationId, messageData.HttpString);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Routing threw an exception");
        }

        await next(context);
    }
}