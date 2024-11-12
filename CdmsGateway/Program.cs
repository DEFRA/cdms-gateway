using CdmsGateway.Utils;
using CdmsGateway.Utils.Logging;
using CdmsGateway.Utils.Mongo;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;
using CdmsGateway.Config;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var app = CreateWebApplication(args);
await app.RunAsync();
return;

[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureWebApplication(builder);

    var app = builder.BuildWebApplication();

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

    ConfigureMongoDb(builder);

    builder.ConfigureEndpoints();

    builder.AddServices(logger);
}

[ExcludeFromCodeCoverage]
static Logger ConfigureLogging(WebApplicationBuilder builder)
{
   builder.Logging.ClearProviders();
   var logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .Enrich.With<LogLevelMapper>()
       .WriteTo.OpenTelemetry(options =>
       {
           options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
           options.ResourceAttributes.Add("service.name", "cdms-gateway");
       })
       .CreateLogger();
   builder.Logging.AddSerilog(logger);
   logger.Information("Starting application");
   return logger;
}

[ExcludeFromCodeCoverage]
static void ConfigureMongoDb(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<IMongoDbClientFactory>(_ =>
        new MongoDbClientFactory(builder.Configuration.GetValue<string>("Mongo:DatabaseUri")!,
            builder.Configuration.GetValue<string>("Mongo:DatabaseName")!));
}
