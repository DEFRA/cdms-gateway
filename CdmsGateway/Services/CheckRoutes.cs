using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using CdmsGateway.Services.Routing;
using CdmsGateway.Utils.Http;

namespace CdmsGateway.Services;

public class CheckRoutes(IMessageRoutes messageRoutes, IHttpClientFactory clientFactory)
{
    public async Task Check(HttpResponse response)
    {
        var results = await Task.WhenAll(messageRoutes.HealthUrls.Select(Check));
        await SetResponse(response, results);
    }

    private async Task<string> Check(HealthUrl healthUrl)
    {
        var client = clientFactory.CreateClient(Proxy.ProxyClient);
        var request = new HttpRequestMessage(new HttpMethod(healthUrl.Method), healthUrl.Url);
        var response = await client.SendAsync(request);
        var traceRoute = TraceRoute(healthUrl.Url);
        return $"{healthUrl.Name} - {healthUrl.Method} {healthUrl.Url} - {response.StatusCode}\r{traceRoute}";
    }

    private static async Task SetResponse(HttpResponse response, string[] results)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = MediaTypeNames.Text.Plain;
        await response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(string.Join("\r", results))));
    }

    private static string TraceRoute(string url)
    {
        var uri = new Uri(url);
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? RunProcess("cmd.exe", $"/c tracert {uri.Host}")
                .Replace("\r\n", "\r")
                .Replace("\rTracing", "Tracing")
                .Replace("]\rover", "] over")
                .Replace("\r\r", "\r")
                .Replace("Trace complete.\r", "")
            : RunProcess("/bin/bash", $"-c \"traceroute {uri.Host}\"");
    }

    private static string RunProcess(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
            
        using var process = Process.Start(startInfo);
        using var reader = process?.StandardOutput;
        return reader?.ReadToEnd() ?? string.Empty;
    }
}