using System.Net.Http.Headers;
using System.Text;
using CdmsGateway.Utils.Http;

namespace CdmsGateway.Services;

public interface ICdsRouter
{
    Task RouteCdsToAlvs(string? clearanceRequestXml);
}

public class MessageRouter(IHttpClientFactory clientFactory) : ICdsRouter
{
    private readonly HttpClient _client = clientFactory.CreateClient(Proxy.ProxyClient);

    public async Task RouteCdsToAlvs(string? clearanceRequestXml)
    {
        await _client.PostAsync("http://localhost:5000/api/v1/clearance-request", new StringContent(clearanceRequestXml, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/xml")));
    }

    public async Task<bool> Route(string from, string message)
    {
        string? to = from.ToLower().Trim('/') switch
        {
            "alvs-apaffs" => "alvs-apaffs-stub",
            "cds" => "cds-stub",
            "alvs-cds" => "alvs-cds-stub",
            _ => null
        };

        if (to == null) return false;
        
        await _client.PostAsync($"http://localhost:5000/{to}", new StringContent(message, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/xml")));
        return true;
    }
}

public class MessageRoutes
{
    
}