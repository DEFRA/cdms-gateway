using System.Text.Json;

namespace CdmsGateway.Services.Checking;

public static class CheckRoutesEndpoints
{
    public const string Path = "checkroutes";
    
    public static void UseCheckRoutesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(Path, CheckRoutes).AllowAnonymous();
        app.MapGet($"/{Path}/json", CheckRoutesAsJson).AllowAnonymous();
    }

    private static async Task<IResult> CheckRoutes(CheckRoutes checkRoutes)
    {
        var results = await checkRoutes.Check();
        return TypedResults.Text($"Maximum time for all tracing {Checking.CheckRoutes.OverallTimeoutSecs} secs.\r\r" +
                                 $"{string.Join('\r', results.Select(result => $"{result.RouteName} - {result.RouteMethod} {result.RouteUrl} - {result.ResponseResult}\r"))}");
    }

    private static async Task<IResult> CheckRoutesAsJson(CheckRoutes checkRoutes)
    {
        var results = await checkRoutes.Check();
        return TypedResults.Json(results);
    }
}