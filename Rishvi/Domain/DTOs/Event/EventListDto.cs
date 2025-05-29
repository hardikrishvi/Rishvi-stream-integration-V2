namespace Rishvi.Domain.DTOs.Event
{
    public class EventListDto
    {
        public int id { get; set; }
        public string event_code { get; set; }
        public string event_code_desc { get; set; }
        public string event_desc { get; set; }
        public string event_date { get; set; }
        public string event_time { get; set; }
        public string event_text { get; set; }
        public string event_link { get; set; }
    }
}
