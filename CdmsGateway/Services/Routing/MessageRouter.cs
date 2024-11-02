using System.Net;
using CdmsGateway.Utils.Http;

namespace CdmsGateway.Services.Routing;

public interface IMessageRouter
{
    Task<RoutingResult> Route(MessageData messageData);
}

public class MessageRouter(IHttpClientFactory clientFactory, IMessageRoutes messageRoutes) : IMessageRouter
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
            
            var content = await response.Content.ReadAsStringAsync();
            return routingResult with { RoutedSuccessfully = response.IsSuccessStatusCode, ResponseContent = content, StatusCode = response.StatusCode, ResponseDate = response.Headers.Date };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return routingResult with { StatusCode = HttpStatusCode.ServiceUnavailable };
        }
    }
}