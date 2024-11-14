using CdmsGateway.Services.Routing;

namespace CdmsGateway.Services.Checking;

public static class CheckRoutesEndpoints
{
    public static readonly string[] Paths = ["testroutes", "test-routes", "checkroutes", "check-routes"];
    
    public static void UseCheckRoutesEndpoints(this IEndpointRouteBuilder app)
    {
        foreach (var path in Paths)
        {
            app.MapGet(path, CheckRoutes).AllowAnonymous();
            app.MapPost(path, CheckRoutes).AllowAnonymous();
        }
    }

    private static async Task<IResult> CheckRoutes(HttpContext context, RoutingConfig routingConfig, CheckRoutes checkRoutes)
    {
        var results = await checkRoutes.Check();
        return TypedResults.Text(results.FormatTraceRoutes());
    }
}