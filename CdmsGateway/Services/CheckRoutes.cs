using System.Net;
using System.Net.Mime;
using System.Text;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils.Http;

namespace CdmsGateway.Services;

public class CheckRoutes(IMessageRoutes messageRoutes, IHttpClientFactory clientFactory)
{
    public async Task Check(HttpResponse response)
    {
        var results = await Task.WhenAll(messageRoutes.HealthUrls.Select(Check));
        await SetResponse(response, results);
    }

    private async Task<string> Check(HealthUrl healthUrl)
    {
        var client = clientFactory.CreateClient(Proxy.ProxyClient);
        var request = new HttpRequestMessage(new HttpMethod(healthUrl.Method), healthUrl.Url);
        var response = await client.SendAsync(request);
        return $"{healthUrl.Name} - {healthUrl.Method} {healthUrl.Url} - {response.StatusCode}";
    }

    private async Task SetResponse(HttpResponse response, string[] results)
    {
        await response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(string.Join("\r", results))));
        response.ContentType = MediaTypeNames.Text.Plain;
        response.StatusCode = (int)HttpStatusCode.OK;
    }

}