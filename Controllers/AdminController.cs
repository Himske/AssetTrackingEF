using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AssetTrackingEF.Controllers {
    internal class AdminController {
        private const string CurrencyRatesUrl = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/usd.json";
        private static readonly HttpClient _httpClient = new();
        private readonly Data.AppDbContext _context;
        public AdminController(Data.AppDbContext context) {
            _context = context;
        }
        public int DeleteAllAssets() {
            var assets = _context.Assets.ToList();
            if (assets.Count == 0) {
                return 0;
            }
            _context.Assets.RemoveRange(assets);
            _context.SaveChanges();
            return assets.Count;
        }

        public int DeleteAllOffices() {
            var offices = _context.Offices.ToList();
            if (offices.Count == 0) {
                return 0;
            }
            _context.Offices.RemoveRange(offices);
            _context.SaveChanges();
            return offices.Count;
        }

        public int DeleteAllCurrencyRates() {
            var currencyRates = _context.CurrencyRates.ToList();
            if (currencyRates.Count == 0) {
                return 0;
            }
            _context.CurrencyRates.RemoveRange(currencyRates);
            _context.SaveChanges();
            return currencyRates.Count;
        }

        public void RefreshCurrencyRates() {
            List<Models.CurrencyRate> currencyRates;
            try {
                var json = _httpClient.GetStringAsync(CurrencyRatesUrl).GetAwaiter().GetResult();
                currencyRates = ParseCurrencyRates(json);
            } catch (HttpRequestException) {
                currencyRates = LoadBundledCurrencyRates();
            } catch (TaskCanceledException) {
                currencyRates = LoadBundledCurrencyRates();
            } catch (JsonException) {
                currencyRates = LoadBundledCurrencyRates();
            }

            var existingRates = _context.CurrencyRates.ToList();
            _context.CurrencyRates.RemoveRange(existingRates);
            _context.CurrencyRates.AddRange(currencyRates);
            _context.SaveChanges();
        }

        private static List<Models.CurrencyRate> LoadBundledCurrencyRates() {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "usd.json");
            var json = File.ReadAllText(filePath);
            return ParseCurrencyRates(json);
        }

        private static List<Models.CurrencyRate> ParseCurrencyRates(string json) {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("usd", out var ratesElement)
                || ratesElement.ValueKind != JsonValueKind.Object) {
                throw new JsonException("The currency rates response does not contain a valid USD rates object.");
            }

            var rates = new List<Models.CurrencyRate>();
            foreach (var rateElement in ratesElement.EnumerateObject()) {
                if (rateElement.Value.ValueKind != JsonValueKind.Number
                    || !rateElement.Value.TryGetDecimal(out var rate)
                    || rate <= 0) {
                    throw new JsonException($"The currency rate for '{rateElement.Name}' is invalid.");
                }

                rates.Add(new Models.CurrencyRate {
                    Currency = rateElement.Name,
                    Rate = rate
                });
            }

            if (rates.Count == 0) {
                throw new JsonException("The currency rates response contains no rates.");
            }

            return rates;
        }
    }
}
