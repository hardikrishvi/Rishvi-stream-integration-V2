using Rishvi.Domain.DTOs.Event;
using Rishvi.Domain.DTOs.Run;
using Rishvi.Domain.DTOs.Subscription;

namespace Rishvi.Domain.DTOs.Webhook
{
    public class WebhookDto
    {
        public WebhookDto()
        {
            subscription = new SubscriptionDto();
            @event = new EventDto();
            run = new RunDto();
            orders = new List<WebhookOrderDto>();
        }
        public SubscriptionDto subscription { get; set; }
        public EventDto @event { get; set; }
        public RunDto run { get; set; }
        public List<WebhookOrderDto> orders { get; set; }
    }
}
