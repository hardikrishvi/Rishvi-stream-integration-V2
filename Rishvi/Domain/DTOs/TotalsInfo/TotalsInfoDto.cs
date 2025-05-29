namespace Rishvi.DTOs.TotalsInfo;

public class TotalsInfoDto
{
    public double? Subtotal { get; set; }
    public double? PostageCost { get; set; }
    public double? PostageCostExTax { get; set; }
    public double? Tax { get; set; }
    public double? TotalCharge { get; set; }
    public string PaymentMethod { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public double? ProfitMargin { get; set; }
    public double? TotalDiscount { get; set; }
    public string Currency { get; set; }
    public double? CountryTaxRate { get; set; }
    public double? ConversionRate { get; set; }
}