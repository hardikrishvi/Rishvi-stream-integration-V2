using Rishvi.Modules.Core.Filters;

namespace Rishvi.DTOs.Address;

public class AddressFilterDto: BaseFilterDto
{
    public AddressFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public string EmailAddress { get; set; }
    public string Town { get; set; }
    public string Region { get; set; }
    public string PostCode { get; set; }
    public string Country { get; set; }
    public string Continent { get; set; }
    public string FullName { get; set; }
    public string Company { get; set; }
    public string PhoneNumber { get; set; }
}