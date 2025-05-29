namespace Rishvi.DTOs.GeneralInfo;

public class GeneralInfoListDto
{
    public Guid Id { get; set; }
    public int? Status { get; set; }
    public bool? LabelPrinted { get; set; }
    public string LabelError { get; set; }
    public bool? InvoicePrinted { get; set; }
    public bool? PickListPrinted { get; set; }
    public bool? IsRuleRun { get; set; }
    public int? Notes { get; set; }
    public bool? PartShipped { get; set; }
    public int? Marker { get; set; }
    public bool? IsParked { get; set; }
    public object Identifiers { get; set; }
    public string ReferenceNum { get; set; }
    public string SecondaryReference { get; set; }
    public string ExternalReferenceNum { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string Source { get; set; }
    public string SubSource { get; set; }
    public string SiteCode { get; set; }
    public bool? HoldOrCancel { get; set; }
    public DateTime? DespatchByDate { get; set; }
    public object ScheduledDelivery { get; set; }
    public bool? HasScheduledDelivery { get; set; }
    public Guid? Location { get; set; }
    public int? NumItems { get; set; }
}