using CdmsGateway.Utils;
using CdmsGateway.Utils.Http;
using CdmsGateway.Utils.Logging;
using CdmsGateway.Utils.Mongo;
using FluentValidation;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;
using CdmsGateway.Services;

//-------- Configure the WebApplication builder------------------//

var app = CreateWebApplication(args);
await app.RunAsync();

[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
   var builder = WebApplication.CreateBuilder(args);

   ConfigureWebApplication(builder);

   var app = BuildWebApplication(builder);

   return app;
}

[ExcludeFromCodeCoverage]
static void ConfigureWebApplication(WebApplicationBuilder builder)
{
   builder.Configuration.AddEnvironmentVariables();
   builder.Configuration.AddIniFile("Properties/local.env", true);

   var logger = ConfigureLogging(builder);

   // Load certificates into Trust Store - Note must happen before Mongo and Http client connections
   builder.Services.AddCustomTrustStore(logger);

   ConfigureMongoDb(builder);

   ConfigureEndpoints(builder);
   
   builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

   builder.Services.AddHttpClient();

   // calls outside the platform should be done using the named 'proxy' http client.
   builder.Services.AddHttpProxyClient(logger);

   builder.Services.AddValidatorsFromAssemblyContaining<Program>();
}

[ExcludeFromCodeCoverage]
static Logger ConfigureLogging(WebApplicationBuilder builder)
{
   builder.Logging.ClearProviders();
   var logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .Enrich.With<LogLevelMapper>()
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

[ExcludeFromCodeCoverage]
static void ConfigureEndpoints(WebApplicationBuilder builder)
{
   builder.Services.AddHealthChecks();
}

[ExcludeFromCodeCoverage]
static WebApplication BuildWebApplication(WebApplicationBuilder builder)
{
   var app = builder.Build();

   app.UseMiddleware<SoapInterceptorMiddleware>();
   
   app.MapHealthChecks("/health");

   return app;
}
