namespace Rishvi.Modules.Core.DTOs
{
    public class LinnworkAuthDto
    {
        public int MaxImagesPerStockItem { get; set; }
        public string FullName { get; set; }
        public string Company { get; set; }
        public string ProductName { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int SessionUserId { get; set; }
        public object DseName { get; set; }
        public string ConnectionString { get; set; }
        public bool IsAccountHolder { get; set; }
        public Guid? Id { get; set; }
        public string EntityId { get; set; }
        public string DatabaseName { get; set; }
        public string sid_registration { get; set; }
        public string UserName { get; set; }
        public string Md5Hash { get; set; }
        public string Locality { get; set; }
        public bool SuperAdmin { get; set; }
        public Guid? Token { get; set; }
        public string GroupName { get; set; }
        public string Device { get; set; }
        public string DeviceType { get; set; }
        public string UserType { get; set; }
        public Status Status { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Server { get; set; }
        public string PushServer { get; set; }
        public object DatabaseServer { get; set; }
        public object DatabaseUser { get; set; }
        public object DatabasePassword { get; set; }
        public object TTL { get; set; }
    }

    public class Status
    {
        public string State { get; set; }
        public string Reason { get; set; }
    }

    //public class RestResponseContent
    //{
    //    public string Code { get; set; }
    //    public string Message { get; set; }
    //}
}
