using System.Net;
using CdmsGateway.Utils.Http;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services.Routing;

public interface IMessageRouter
{
    Task<RoutingResult> Route(MessageData messageData);
    Task<RoutingResult> Fork(MessageData messageData);
}

public class MessageRouter(IHttpClientFactory clientFactory, IMessageRoutes messageRoutes, ILogger logger) : IMessageRouter
{
    public async Task<RoutingResult> Route(MessageData messageData)
    {
        var routingResult = messageRoutes.GetRoutedRoute(messageData.Path);
        return await Route(messageData, routingResult);
    }
    
    public async Task<RoutingResult> Fork(MessageData messageData)
    {
        var routingResult = messageRoutes.GetForkedRoute(messageData.Path);
        return await Route(messageData, routingResult);
    }
    
    private async Task<RoutingResult> Route(MessageData messageData, RoutingResult routingResult)
    {
        if (!routingResult.RouteFound) return routingResult;
        
        try
        {
            var client = clientFactory.CreateClient(Proxy.ProxyClient);
            var request = messageData.CreateForwardingRequest(routingResult.RouteUrl);
            var response = await client.SendAsync(request);
            
            var content = await response.Content.ReadAsStringAsync();
            return routingResult with { RoutedSuccessfully = response.IsSuccessStatusCode, ResponseContent = content, StatusCode = response.StatusCode, ResponseDate = response.Headers.Date };
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error routing");
            return routingResult with { StatusCode = HttpStatusCode.ServiceUnavailable };
        }
    }
}