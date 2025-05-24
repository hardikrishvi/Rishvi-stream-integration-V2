using System.Text.Json;

namespace Rishvi.Modules.Users.Models.DTOs
{
    public class CreateOrderViewRequestDto
    {
        public string viewName { get; set; }
        public string OrderViewDetailJSON { get; set; }
    }
}
