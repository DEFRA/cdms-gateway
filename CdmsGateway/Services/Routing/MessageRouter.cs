using System.Net;
using System.Net.Mime;
using System.Text;
using CdmsGateway.Utils.Http;

namespace CdmsGateway.Services.Routing;

public interface IMessageRouter
{
    Task<RoutingResult> Route(string from, string message, string correlationId);
}

public class MessageRouter(IHttpClientFactory clientFactory, IMessageRoutes messageRoutes) : IMessageRouter
{
    public async Task<RoutingResult> Route(string from, string message, string correlationId)
    {
        var routeUrl = messageRoutes.GetRoute(from);

        if (routeUrl == null) return new RoutingResult();

        try
        {
            var client = clientFactory.CreateClient(Proxy.ProxyClient);
            client.DefaultRequestHeaders.Add("x-correlation-id", correlationId);
            var response = await client.PostAsync(routeUrl, new StringContent(message, Encoding.UTF8, MediaTypeNames.Application.Soap));
            var content = await response.Content.ReadAsStringAsync();
            return new RoutingResult { RouteFound = true, RouteUrl = routeUrl, RoutedSuccessfully = response.IsSuccessStatusCode, ResponseContent = content, StatusCode = response.StatusCode };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new RoutingResult { RouteFound = true, RouteUrl = routeUrl, StatusCode = HttpStatusCode.ServiceUnavailable };
        }
    }
}