using System.Net;
using System.Net.Mime;
using System.Text;
using CdmsGateway.Utils.Http;

namespace CdmsGateway.Services.Routing;

public interface IMessageRouter
{
    Task<RoutingResult> Route(string from, string message);
}

public class MessageRouter(IHttpClientFactory clientFactory, IMessageRoutes messageRoutes) : IMessageRouter
{
    private readonly HttpClient _client = clientFactory.CreateClient(Proxy.ProxyClient);

    public async Task<RoutingResult> Route(string from, string message)
    {
        var routeDefinition = messageRoutes.GetRoute(from);

        if (routeDefinition.IsEmpty) return new RoutingResult();

        try
        {
            var response = await _client.PostAsync(routeDefinition.Url, new StringContent(message, Encoding.UTF8, MediaTypeNames.Application.Soap));
            var content = await response.Content.ReadAsStringAsync();
            return new RoutingResult { RouteFound = true, RouteUrl = routeDefinition.Url, RoutedSuccessfully = response.IsSuccessStatusCode, ResponseContent = content, StatusCode = response.StatusCode };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new RoutingResult { RouteFound = true, RouteUrl = routeDefinition.Url, RoutedSuccessfully = false, StatusCode = HttpStatusCode.ServiceUnavailable };
        }
    }
}