namespace Rishvi.Modules.Core.Helpers
{
    public static class DateTimeHelper
    {
        public static string TimeAgo(this DateTime date)
        {
            TimeSpan timeSpan = DateTime.Now.Subtract(date);
            if (timeSpan.TotalMilliseconds < 1.0)
            {
                return "not yet";
            }

            if (timeSpan.TotalMinutes < 1.0)
            {
                return "just now";
            }

            if (timeSpan.TotalMinutes < 2.0)
            {
                return "1 minute ago";
            }

            if (timeSpan.TotalMinutes < 60.0)
            {
                return $"{timeSpan.Minutes} minutes ago";
            }

            if (timeSpan.TotalMinutes < 120.0)
            {
                return "1 hour ago";
            }

            if (timeSpan.TotalHours < 24.0)
            {
                return $"{timeSpan.Hours} hours ago";
            }

            if (timeSpan.TotalDays == 1.0)
            {
                return "yesterday";
            }

            if (timeSpan.TotalDays < 7.0)
            {
                return $"{timeSpan.Days} days ago";
            }

            if (timeSpan.TotalDays < 14.0)
            {
                return "last week";
            }

            if (timeSpan.TotalDays < 21.0)
            {
                return "2 weeks ago";
            }

            if (timeSpan.TotalDays < 28.0)
            {
                return "3 weeks ago";
            }

            if (timeSpan.TotalDays < 60.0)
            {
                return "last month";
            }

            if (timeSpan.TotalDays < 365.0)
            {
                return $"{Math.Round(timeSpan.TotalDays / 30.0)} months ago";
            }

            return timeSpan.TotalDays < 730.0
                    ? "last year"
                    : $"{Math.Round(timeSpan.TotalDays / 365.0)} years ago";
        }

        public static double ToUnixTimeStamp(this DateTime date)
        {
            return (date - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds;
        }
    }
}
