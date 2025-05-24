namespace Rishvi.Modules.Core.DTOs
{
    public class DateTimeDto
    {
        public DateTimeDto()
        {
        }

        public DateTimeDto(int year, int month, int day)
        {
            this.Year = year;
            this.Month = month;
            this.Day = day;
        }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }
}
