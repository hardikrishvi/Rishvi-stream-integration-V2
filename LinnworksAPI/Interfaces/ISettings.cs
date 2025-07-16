using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public interface ISettingsController
    {
        void DeleteCurrencyConversionRates(List<String> currencies);
        GetAvailableTimeZonesResponse GetAvailableTimeZones();
        List<CurrencyConversionRate> GetCurrencyConversionRates(GetConversionRatesRequest requestParams);
        GetLatestCurrencyRateResponse GetLatestCurrencyRate(GetLatestCurrencyRateRequest request);
        Measures GetMeasures();
        void InsertCurrencyConversionRates(List<CurrencyConversionRate> rates);
        void UpdateCurrencyConversionRates(List<CurrencyConversionRate> rates);
    }
}