using System.Runtime.Serialization;

namespace CdmsGateway.Future;

[DataContract]
public class ServiceHeader
{
    [DataMember]
    public string SourceSystem { get; set; }

    [DataMember]
    public string DestinationSystem { get; set; }

    [DataMember]
    public string CorrelationId { get; set; }

    [DataMember]
    public DateTime ServiceCallTimestamp { get; set; }
}