using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CdmsGateway.Future;

[DataContract]
public class Envelope
{
    [JsonIgnore]
    public string? OriginalSoapMessage { get; set; }
}