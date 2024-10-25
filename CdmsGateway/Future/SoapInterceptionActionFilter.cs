using System.Runtime.Serialization;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CdmsGateway.Future;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class SoapInterceptionActionFilter<T> : Attribute, IAsyncActionFilter where T : Envelope
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        string xml;
        using (var reader = new StreamReader(context.HttpContext.Request.Body))
        {
            xml = await reader.ReadToEndAsync();
        }

        var serializer = new DataContractSerializer(typeof(T));
        
        using (var stringReader = new StringReader(xml))
        using (var xmlReader = XmlReader.Create(stringReader))
        {
            if (serializer.ReadObject(xmlReader) is T envelope)
            {
                envelope.OriginalSoapMessage = xml;
                context.ActionArguments["envelope"] = envelope;
            }
        }

        await next();
    }
}