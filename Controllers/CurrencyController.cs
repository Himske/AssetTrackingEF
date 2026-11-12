using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AssetTrackingEF.Controllers {
    internal class CurrencyController {
        private readonly Data.AppDbContext _context;
        
        public CurrencyController(Data.AppDbContext context) {
            _context = context;
        }

        public decimal ConvertCurrency(string toCurrency, decimal amount) {
            var toRate = _context.CurrencyRates.FirstOrDefault(c => c.Currency == toCurrency);
            if (toRate == null) {
                throw new ArgumentException("Invalid currency code.");
            }
            decimal convertedAmount = amount * toRate.Rate;
            return convertedAmount;
        }

        public static string FormatPrice(decimal amount, string currencyCode) {
            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ArgumentException("Currency code cannot be null or empty.", nameof(currencyCode));

            // Find a culture that uses this currency code
            var culture = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new { Culture = c, Region = new RegionInfo(c.Name) })
                .FirstOrDefault(x => x.Region.ISOCurrencySymbol.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));

            if (culture == null)
                throw new ArgumentException($"No culture found for currency code '{currencyCode}'.", nameof(currencyCode));

            // Format using the found culture
            return string.Format(culture.Culture, "{0:C}", amount);
        }
    }
}
