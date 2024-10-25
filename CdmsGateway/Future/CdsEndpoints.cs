using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CdmsGateway.Future;

public class CdsEndpoints(ICdsRouter router) : ControllerBase
{
    // [HttpPost("CdsService")]
    // [TypeFilter(typeof(SoapInterceptionActionFilter<EnvelopeALVSClearanceRequest>))]
    public async Task<IActionResult>  AlvsClearanceRequest(EnvelopeALVSClearanceRequest envelope)
    {
        await router.RouteClearanceRequest(envelope.OriginalSoapMessage);

        Console.WriteLine(JsonSerializer.Serialize(envelope, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }));

        return Ok();
    }
}