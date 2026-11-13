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

            var culture = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new { Culture = c, Region = new RegionInfo(c.Name) })
                .FirstOrDefault(x => x.Region.ISOCurrencySymbol.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));

            if (culture == null)
                throw new ArgumentException($"No culture found for currency code '{currencyCode}'.", nameof(currencyCode));

            var formattingCulture = (CultureInfo)culture.Culture.Clone();
            if (currencyCode.Equals("EUR", StringComparison.OrdinalIgnoreCase)) {
                return $"{amount.ToString("N2", formattingCulture)} \u20AC";
            }

            var currencySymbol = currencyCode.Equals("JPY", StringComparison.OrdinalIgnoreCase)
                ? "¥"
                : culture.Region.CurrencySymbol;
            formattingCulture.NumberFormat.CurrencySymbol = currencySymbol;
            return amount.ToString("C", formattingCulture);
        }
    }
}
