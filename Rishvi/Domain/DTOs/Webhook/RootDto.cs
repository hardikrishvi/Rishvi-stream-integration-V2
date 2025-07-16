namespace Rishvi.Domain.DTOs.Webhook
{
    public class RootDto
    {
        public RootDto()
        {
            webhook = new WebhookDto();
        }
        public WebhookDto webhook { get; set; }
    }
}
