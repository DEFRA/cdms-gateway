using CdmsGateway.Services.Routing;
using CdmsGateway.Utils;

namespace CdmsGateway.Services;

public class SoapInterceptorMiddleware(RequestDelegate next, IMessageRouter messageRouter)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        if (request.Method == HttpMethods.Post && request.Path.HasValue)
        {
            request.EnableBuffering();
            var messageBody = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;
            
            Console.WriteLine(request.HttpString());
            Console.WriteLine(messageBody);

            var routingResult = await messageRouter.Route(request.Path, messageBody);
            
            if (routingResult.RouteFound)
            {
                Console.WriteLine(routingResult.RoutedSuccessfully ? $"Successfully routed to {routingResult.RouteUrl}" : $"Failed to route to {routingResult.RouteUrl} with response code {routingResult.ResponseCode}");
                return;
            }

            Console.WriteLine("Routing failed");
        }

        await next(context);
    }
}
