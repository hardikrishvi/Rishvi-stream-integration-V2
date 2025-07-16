using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Filters
    {
        public List<NumericFilter> NumericFilters { get; set; }

        public BooleanFilter BooleanFilter { get; set; }

        public List<DateRangeFilter> DateRangeFilters { get; set; }

        public List<StringFilter> StringFilters { get; set; }
    }
}