namespace Rishvi.Modules.Core.Filters
{
    public abstract class BaseFilterDto
    {
        public int Page { get; set; }
        public int Size { get; set; }

        public string SortColumn { get; set; }
        public string SortType { get; set; }

        public DateTime? FromCreatedAt { get; set; }
        public DateTime? ToCreatedAt { get; set; }

        public DateTime? FromUpdatedAt { get; set; }
        public DateTime? ToUpdatedAt { get; set; }
    }
}