using CdmsGateway.Services.Routing;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services;

public class SoapInterceptorMiddleware(RequestDelegate next, IMessageRouter messageRouter, IMessageFork messageFork, ILogger logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var messageData = await MessageData.Create(context.Request, logger);
            if (messageData.ShouldProcessRequest())
            {
                logger.Information("{CorrelationId} Received routing instruction {HttpString} {Content}", messageData.CorrelationId, messageData.HttpString, messageData.ContentAsString);

                #pragma warning disable CS4014 // This call is not awaited as forking of the message should happen asynchronously
                Fork(messageData);
                #pragma warning restore CS4014

                await Route(context, messageData);
            }
            
            logger.Information("{CorrelationId} Pass through request {HttpString}", messageData.CorrelationId, messageData.HttpString);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "There was a routing error");
        }

        await next(context);
    }

    private async Task Route(HttpContext context, MessageData messageData)
    {
        const string Action = "Routing";
        var routingResult = await messageRouter.Route(messageData);

        if (routingResult.RouteFound)
        {
            CheckResults(messageData, routingResult, Action);
            if (routingResult.RoutedSuccessfully) await messageData.PopulateResponse(context.Response, routingResult);
        }
        else
        {
            logger.Information("{CorrelationId} {Action} not supported for [{HttpString}]", messageData.CorrelationId, Action, messageData.HttpString);
        }
    }

    private async Task Fork(MessageData messageData)
    {
        const string Action = "Forking";
        var routingResult = await messageRouter.Fork(messageData);

        if (routingResult.RouteFound)
        {
            CheckResults(messageData, routingResult, Action);
            messageFork.Complete();
        }
        else
        {
            logger.Information("{CorrelationId} {Action} not supported for [{HttpString}]", messageData.CorrelationId, Action, messageData.HttpString);
        }
    }

    private void CheckResults(MessageData messageData, RoutingResult routingResult, string action)
    {
        if (routingResult.RoutedSuccessfully)
        {
            logger.Information("{CorrelationId} {Action} successful for route {RouteUrl} with response {StatusCode} {Content}", messageData.CorrelationId, action, routingResult.RouteUrl, routingResult.StatusCode, routingResult.ResponseContent);
        }
        else
        {
            logger.Information("{CorrelationId} {Action} failed for route {RouteUrl} with status code {StatusCode}", messageData.CorrelationId, action, routingResult.RouteUrl, routingResult.StatusCode);
        }
    }
}