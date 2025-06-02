using Rishvi.Modules.Core.Filters;
using Rishvi.Modules.Core.ListOrders;

namespace Rishvi.Domain.DTOs.Authorization
{
    public class AuthorizationFilterDto : BaseFilterDto
    {
        public AuthorizationFilterDto()
        {
            SortColumn = "CreatedAt";
            SortType = "DESC";
        }
        public string AddressLine1 { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string ContactName { get; set; }
        public string ContactPhoneNo { get; set; }
        public string CountryCode { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
    }
}
