namespace Rishvi.Modules.ErrorLogs.Models.DTOs
{
    public class SystemLogListDto
    {
        public Guid SystemLogID { get; set; }
        public string ModuleName { get; set; }
        public string RequestHeader { get; set; }
        public string RequestJson { get; set; }
        public string ResponseJson { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
