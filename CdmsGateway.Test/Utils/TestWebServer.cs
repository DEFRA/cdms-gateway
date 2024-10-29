using CdmsGateway.Config;
using CdmsGateway.Utils.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace CdmsGateway.Test.Utils;

public class TestWebServer : IAsyncDisposable
{
    private static int _portNumber = 5000;
    
    private readonly WebApplication _app;

    public TestHttpHandler TestHttpHandler { get; }
    public HttpClient HttpClient { get; }
    public IServiceProvider Services { get; }

    public static TestWebServer BuildAndRun(params ServiceDescriptor[] testServices) => new(testServices);

    private TestWebServer(params ServiceDescriptor[] testServices)
    {
        TestHttpHandler = new TestHttpHandler();
        var url = $"http://localhost:{_portNumber++}/";
        HttpClient = new HttpClient { BaseAddress = new Uri(url) };
        
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(url);
        builder.AddServices(Substitute.For<Serilog.ILogger>());
        foreach (var testService in testServices) builder.Services.Replace(testService);
        builder.ConfigureEndpoints();

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var httpClient = new HttpClient(TestHttpHandler);
        httpClientFactory.CreateClient(Proxy.ProxyClient).Returns(httpClient);
        builder.Services.Replace(ServiceDescriptor.Singleton(httpClientFactory));

        _app = builder.BuildWebApplication();
        Services = _app.Services;
        
        _app.RunAsync();
    }

    public async ValueTask DisposeAsync() => await _app.DisposeAsync();
}