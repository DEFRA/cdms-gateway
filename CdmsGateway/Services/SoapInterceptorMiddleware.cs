namespace CdmsGateway.Services;

public class SoapInterceptorMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Post && context.Request.Path.HasValue)
        {
            context.Request.EnableBuffering();

            var messageBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;
            
            Console.WriteLine(messageBody);
        }

        await next(context);
    }
}
