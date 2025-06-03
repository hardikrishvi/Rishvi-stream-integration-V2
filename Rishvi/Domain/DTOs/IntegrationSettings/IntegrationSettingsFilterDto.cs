using Rishvi.Modules.Core.Filters;

namespace Rishvi.Domain.DTOs.IntegrationSettings;

public class IntegrationSettingsFilterDto : BaseFilterDto
{
    public IntegrationSettingsFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "desc";
    }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string AuthorizationToken { get; set; }
    public string LinnworksSyncToken { get; set; }
}
