using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetLatestCurrencyRateResponse
    {
        public String BaseCurrency { get; set; }

        public List<CurrencyRate> Rates { get; set; }
    }
}