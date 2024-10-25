using System.Net.Http.Headers;
using System.Text;

namespace CdmsGateway.Future;

public interface ICdsRouter
{
    Task RouteClearanceRequest(string? clearanceRequestXml);
}

public class CdsRouter(IHttpClientFactory clientFactory) : ICdsRouter
{
    private readonly HttpClient _client = clientFactory.CreateClient();

    public async Task RouteClearanceRequest(string? clearanceRequestXml)
    {
        await _client.PostAsync("http://localhost:5000/api/v1/clearance-request", new StringContent(clearanceRequestXml, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/xml")));
    }
}