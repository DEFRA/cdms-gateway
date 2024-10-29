using System.Diagnostics.CodeAnalysis;
using CdmsGateway.Services;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils;
using CdmsGateway.Utils.Http;
using FluentValidation;

namespace CdmsGateway.Config;

public static class ConfugreWebApp
{
    [ExcludeFromCodeCoverage]
    public static void AddServices(this WebApplicationBuilder builder, Serilog.ILogger logger)
    {
        builder.ConfigureToType<RouteConfig>("Routes");

        builder.Services.AddHttpClient();
        // calls outside the platform should be done using the named 'proxy' http client.
        builder.Services.AddHttpProxyClient(logger);
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
   
        builder.Services.AddSingleton<IMessageRouter, MessageRouter>();
        builder.Services.AddSingleton<IMessageRoutes, MessageRoutes>();
    }

    [ExcludeFromCodeCoverage]
    public static void ConfigureEndpoints(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
    }

    [ExcludeFromCodeCoverage]
    public static WebApplication BuildWebApplication(this WebApplicationBuilder builder)
    {
        var app = builder.Build();

        app.UseMiddleware<SoapInterceptorMiddleware>();
   
        app.MapHealthChecks("/health");

        return app;
    }
}