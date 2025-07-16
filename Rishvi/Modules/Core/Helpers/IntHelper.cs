namespace Rishvi.Modules.Core.Helpers
{
    public static class IntHelper
    {
        public static bool IsIn(this int[] values, string inValue)
        {
            return ((IEnumerable<int>)values).Any(p => p.Equals(inValue));
        }

        public static bool IsIn(this int value, params int[] inValues)
        {
            return ((IEnumerable<int>)inValues).Any(p => p.Equals(value));
        }

        public static bool IsBetween(this int value, int from, int to)
        {
            return value >= from && value <= to;
        }
    }
}
