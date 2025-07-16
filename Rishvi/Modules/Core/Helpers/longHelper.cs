namespace Rishvi.Modules.Core.Helpers
{
    public static class LongHelper
    {
        public static bool IsIn(this long[] values, string inValue)
        {
            return ((IEnumerable<long>)values).Any(p => p.Equals(inValue));
        }

        public static bool IsIn(this long value, params long[] inValues)
        {
            return ((IEnumerable<long>)inValues).Any(p => p.Equals(value));
        }
    }
}
