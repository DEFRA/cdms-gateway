using System.Net;
using System.Net.Http.Headers;
using System.Text;
using CdmsGateway.Utils.Http;

namespace CdmsGateway.Services.Routing;

public interface IMessageRouter
{
    Task<RoutingResult> Route(string routePath, string message, MessageHeaders messageHeaders);
}

public class MessageRouter(IHttpClientFactory clientFactory, IMessageRoutes messageRoutes) : IMessageRouter
{
    public async Task<RoutingResult> Route(string routePath, string message, MessageHeaders messageHeaders)
    {
        var route = messageRoutes.GetRoute(routePath);
        if (route == default) return new RoutingResult { RouteName = null };
        var routingResult = new RoutingResult { RouteName = route.Name };
        if (route.Url == null) return routingResult;

        routingResult = routingResult with { RouteFound = true, RouteUrl = route.Url };
        try
        {
            var client = clientFactory.CreateClient(Proxy.ProxyClient);
            client.DefaultRequestHeaders.Add("Date", messageHeaders.Date);
            client.DefaultRequestHeaders.Add(MessageHeaders.CorrelationIdName, messageHeaders.CorrelationId);
            client.DefaultRequestHeaders.Add("x-correlation-id", messageHeaders.CorrelationId);
            if (messageHeaders.Authorization != null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(messageHeaders.Authorization);
            var response = await client.PostAsync(route.Url, new StringContent(message, Encoding.UTF8, messageHeaders.ContentType));
            var content = await response.Content.ReadAsStringAsync();
            return routingResult with { RoutedSuccessfully = response.IsSuccessStatusCode, ResponseContent = content, StatusCode = response.StatusCode };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return routingResult with { StatusCode = HttpStatusCode.ServiceUnavailable };
        }
    }
}