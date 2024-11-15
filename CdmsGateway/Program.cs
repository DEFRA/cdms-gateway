using CdmsGateway.Utils;
using CdmsGateway.Utils.Logging;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;
using CdmsGateway.Config;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Environment = System.Environment;

var app = CreateWebApplication(args);
await app.RunAsync();

[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureWebApplication(builder);

    ConfigureSwaggerBuilder(builder);

    var app = builder.BuildWebApplication();

    ConfigureSwaggerApp(app);

    return app;
}

[ExcludeFromCodeCoverage]
static void ConfigureWebApplication(WebApplicationBuilder builder)
{
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddIniFile("Properties/local.env", true);

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddRuntimeInstrumentation()
                   .AddMeter(
                       "Microsoft.AspNetCore.Hosting",
                       "Microsoft.AspNetCore.Server.Kestrel",
                       "System.Net.Http",
                       MetricsHost.Name);
        })
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation();
        })
        .UseOtlpExporter();

    var logger = ConfigureLogging(builder);

    // Load certificates into Trust Store - Note must happen before Mongo and Http client connections
    builder.Services.AddCustomTrustStore(logger);

    builder.ConfigureEndpoints();

    builder.AddServices(logger);
}

[ExcludeFromCodeCoverage]
static Logger ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    var loggerConfiguration = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.With<LogLevelMapper>()
        .Enrich.WithProperty("service.version", Environment.GetEnvironmentVariable("SERVICE_VERSION"));
    if (builder.Environment.IsDevelopment())
        loggerConfiguration.WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            options.ResourceAttributes.Add("service.name", "cdms-gateway");
        });
    var logger = loggerConfiguration.CreateLogger();
    builder.Logging.AddSerilog(logger);
    logger.Information("Starting application");
    return logger;
}

[ExcludeFromCodeCoverage]
static void ConfigureSwaggerBuilder(WebApplicationBuilder builder)
{
    if (builder.IsSwaggerEnabled())
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("public-v0.1", new OpenApiInfo { Title = "TDM Public API", Version = "v1" }); });
    }
}

[ExcludeFromCodeCoverage]
static void ConfigureSwaggerApp(WebApplication app)
{
    if (app.IsSwaggerEnabled())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/public-v0.1/swagger.json", "public");
        });
    }
}