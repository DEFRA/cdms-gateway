using System.Net;
using CdmsGateway.Utils.Http;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Services.Routing;

public interface IMessageRouter
{
    Task<RoutingResult> Route(MessageData messageData);
}

public class MessageRouter(IHttpClientFactory clientFactory, IMessageRoutes messageRoutes, ILogger logger) : IMessageRouter
{
    public async Task<RoutingResult> Route(MessageData messageData)
    {
        var routingResult = messageRoutes.GetRoute(messageData.Path);
        if (!routingResult.RouteFound) return routingResult;

        try
        {
            var client = clientFactory.CreateClient(Proxy.ProxyClient);
            var request = messageData.CreateForwardingRequest(routingResult.RouteUrl);
            var response = await client.SendAsync(request);
            
            foreach (var header in response.Headers.SelectMany(x => x.Value.ToArray(), (x, y) => new { x.Key, Value = y})) 
                logger.Information("RES-HEADER: {Key}={Value}", header.Key, header.Value);
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