using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class PagedStockCategoryLocationProductResult
    {
        public Int64 TotalResults { get; set; }

        public List<StockCategoryLocationProduct> Results { get; set; }
    }
}