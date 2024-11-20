using System.Diagnostics.CodeAnalysis;
using CdmsGateway.Services;
using CdmsGateway.Services.Checking;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils;
using CdmsGateway.Utils.Http;
using FluentValidation;
using Polly;
using Polly.Extensions.Http;
using ILogger = Serilog.ILogger;

namespace CdmsGateway.Config;

public static class ConfigureWebApp
{
    public static IHttpClientBuilder? HttpProxyClientWithRetryBuilder;

    [ExcludeFromCodeCoverage]
    public static void AddServices(this WebApplicationBuilder builder, ILogger logger)
    {
        builder.Services.AddSingleton(logger);
        builder.ConfigureToType<RoutingConfig>("Routing");

        HttpProxyClientWithRetryBuilder = builder.Services.AddHttpProxyClientWithoutRetry(logger);
        HttpProxyClientWithRetryBuilder = builder.Services.AddHttpProxyClientWithRetry(logger)
                                                          .AddPolicyHandler(_ => HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(100)));
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddSingleton<IMessageFork, MessageFork>();
        builder.Services.AddSingleton<IMessageRouter, MessageRouter>();
        builder.Services.AddSingleton<IMessageRoutes, MessageRoutes>();
        builder.Services.AddSingleton<CheckRoutes>();
        builder.Services.AddSingleton<MetricsHost>();
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
        
        app.UseCheckRoutesEndpoints();

        return app;
    }
}