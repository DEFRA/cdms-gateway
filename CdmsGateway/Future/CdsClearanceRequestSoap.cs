using System.Runtime.Serialization;

// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CdmsGateway.Future;

[DataContract(Name = "Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope/")]
public class EnvelopeALVSClearanceRequest : Envelope
{
    [DataMember(Name = "Body")]
    public BodyALVSClearanceRequest Body { get; set; }
}

[DataContract(Name = "Body")]
public class BodyALVSClearanceRequest
{
    [DataMember]
    public SendALVSClearanceRequest SendALVSClearanceRequest { get; set; }
}

[DataContract(Namespace = "http://tempuri.org/")]
public class SendALVSClearanceRequest
{
    [DataMember]
    public ALVSClearanceRequest ALVSClearanceRequest { get; set; }
}

[DataContract(Namespace = "http://submitimportdocumenthmrcfacade.types.esb.ws.cara.defra.com")]
public class ALVSClearanceRequest
{
    [DataMember(Order = 1)]
    public ServiceHeader ServiceHeader { get; set; }

    [DataMember(Order = 2)]
    public ALVSClearanceRequestHeader Header { get; set; }
    
    // [DataMember(Order = 3, IsRequired = true)]
    // public ALVSClearanceRequestItems Items { get; set; }
}

[DataContract]
public class ALVSClearanceRequestHeader
{
    [DataMember]
    public string EntryReference { get; set; }

    [DataMember]
    public string EntryVersionNumber { get; set; }

    [DataMember]
    public string? PreviousVersionNumber { get; set; }

    [DataMember]
    public string DeclarationUCR { get; set; }

    [DataMember]
    public string? DeclarationPartNumber { get; set; }

    [DataMember]
    public string DeclarationType { get; set; }

    [DataMember]
    public DateTime? ArrivalDateTime { get; set; }

    [DataMember]
    public string? SubmitterTURN { get; set; }

    [DataMember]
    public string DeclarantId { get; set; }

    [DataMember]
    public string DeclarantName { get; set; }

    [DataMember]
    public string DispatchCountryCode { get; set; }

    [DataMember]
    public string? GoodsLocationCode { get; set; }

    [DataMember]
    public string MasterUcr { get; set; }
}

// [CollectionDataContract(ItemName = "Item")]
// public class ALVSClearanceRequestItems : List<ALVSClearanceRequestItem> { }
//
// [DataContract]
// public class ALVSClearanceRequestItem
// {
//     [DataMember]
//     public required int ItemNumber { get; set; }
//
//     [DataMember]
//     public required string CustomsProcedureCode { get; set; }
//
//     [DataMember]
//     public required string TaricCommodityCode { get; set; }
//
//     [DataMember]
//     public required string GoodsDescription { get; set; }
//
//     [DataMember]
//     public required string ConsigneeId { get; set; }
//
//     [DataMember]
//     public required string ConsigneeName { get; set; }
//
//     [DataMember]
//     public required string ItemNetMass { get; set; }
//
//     [DataMember]
//     public string? ItemSupplementaryUnits { get; set; }
//
//     [DataMember]
//     public required string ItemThirdQuantity { get; set; }
//
//     [DataMember]
//     public required string ItemOriginCountryCode { get; set; }
// }