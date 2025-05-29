using System.ComponentModel.DataAnnotations;
using Rishvi.Core.Data;

namespace Rishvi.Models;

public class TaxInfo : IModificationHistory
{
    [Key]
    public Guid TaxInfoId { get; set; }
    public string TaxNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}