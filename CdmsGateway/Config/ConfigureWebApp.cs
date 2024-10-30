using System.Diagnostics.CodeAnalysis;
using CdmsGateway.Services;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils;
using CdmsGateway.Utils.Http;
using FluentValidation;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Config;

public static class ConfigureWebApp
{
    [ExcludeFromCodeCoverage]
    public static void AddServices(this WebApplicationBuilder builder, ILogger logger)
    {
        builder.Services.AddSingleton(logger);
        builder.ConfigureToType<RouteConfig>("Routes");

        builder.Services.AddHttpClient();
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