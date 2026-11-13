using AssetTrackingEF.Controllers;
using AssetTrackingEF.Models;
using Spectre.Console;
using System.Globalization;

namespace AssetTrackingEF.Views {
    internal class ReportingView {
        private readonly ReportingController _controller;
        private readonly CurrencyController _currencyController;

        public ReportingView(ReportingController controller, CurrencyController currencyController) {
            _controller = controller;
            _currencyController = currencyController;
        }

        public void ShowReportingMenu() {
            bool inReportingMenu = true;

            while (inReportingMenu) {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[bold cyan]Asset Tracking[/]");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Reporting")
                        .AddChoices([
                            "Assets and Value per Office",
                            "Asset Counts, Status, and Highest Prices",
                            "Back"
                        ]));

                switch (choice) {
                    case "Assets and Value per Office":
                        ShowAssetsAndValuePerOffice();
                        break;
                    case "Asset Counts, Status, and Highest Prices":
                        ShowAssetCountsStatusAndHighestPrices();
                        break;
                    case "Back":
                        inReportingMenu = false;
                        break;
                }
            }
        }

        private void ShowAssetsAndValuePerOffice() {
            var offices = _controller.GetOfficesWithAssets();
            if (offices.Count == 0) {
                AnsiConsole.MarkupLine("[yellow]No offices found.[/]");
                Pause();
                return;
            }

            foreach (var office in offices) {
                var heading = $"{office.Name.ToUpperInvariant()} OFFICE";
                AnsiConsole.MarkupLine($"[bold cyan]=============== {Markup.Escape(heading)} ===============[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("ID   Asset    Brand        Price       Status");
                AnsiConsole.MarkupLine("-------------------------------------------------");

                foreach (var asset in office.Assets.OrderBy(asset => asset.Id)) {
                    var status = asset.GetStatus();
                    var color = status switch {
                        "EXPIRED" => "grey",
                        "RED" => "red",
                        "YELLOW" => "yellow",
                        _ => "green"
                    };
                    var price = FormatOfficeCurrency(asset.PurchasePrice, office.Currency);
                    AnsiConsole.MarkupLine(
                        $"{asset.Id,-4} {Markup.Escape(asset.AssetType),-8} {Markup.Escape(asset.Brand),-12} {Markup.Escape(price),-11} [{color}]{status}[/]");
                }

                var totalValue = FormatOfficeCurrency(office.Assets.Sum(asset => asset.PurchasePrice), office.Currency);
                AnsiConsole.MarkupLine($"\nTotal Office Value: [bold]{Markup.Escape(totalValue)}[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("================================================");
                AnsiConsole.WriteLine();
            }

            Pause();
        }

        private void ShowAssetCountsStatusAndHighestPrices() {
            AnsiConsole.MarkupLine("[bold cyan]===================== REPORT ============================[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[bold]Office Asset Counts[/]");
            AnsiConsole.MarkupLine("---------------------------------------------------------");
            var offices = _controller.GetOfficesWithAssets();
            foreach (var office in offices) {
                AnsiConsole.MarkupLine($"{Markup.Escape(office.Name)} : {office.Assets.Count}");
            }
            if (offices.Count == 0) {
                AnsiConsole.MarkupLine("No offices found.");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Assets Near Or Expired[/]");
            AnsiConsole.MarkupLine("---------------------------------------------------------");
            var assetsToReview = _controller.GetAssetsCloseToOrExpired();
            if (assetsToReview.Count == 0) {
                AnsiConsole.MarkupLine("No assets are near expiration or expired.");
            }
            else {
                var statusRows = assetsToReview.Select(asset => new[] {
                    asset.GetStatus(),
                    $"{asset.Brand} {asset.ModelName}",
                    asset.Office.Name,
                    asset.PurchaseDate.ToString("MM/dd/yyyy", CultureInfo.GetCultureInfo("en-US"))
                }).ToList();
                WriteTextTable(["Status", "Asset", "Office", "Purchase Date"], statusRows);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Most Expensive Assets[/]");
            AnsiConsole.MarkupLine("---------------------------------------------------------");
            var mostExpensiveAssets = _controller.GetAssetsByPurchasePrice();
            if (mostExpensiveAssets.Count == 0) {
                AnsiConsole.MarkupLine("No assets found.");
            }
            else {
                var priceRows = mostExpensiveAssets
                    .Select((asset, index) => new[] {
                        (index + 1).ToString(CultureInfo.InvariantCulture),
                        $"{asset.Brand} {asset.ModelName}",
                        asset.Office.Name,
                        FormatUsd(asset.PurchasePrice)
                    })
                    .ToList();
                WriteTextTable(["Rank", "Asset", "Office", "Purchase Price (USD)"], priceRows);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold cyan]=========================================================[/]");
            Pause();
        }

        private static void WriteTextTable(IReadOnlyList<string> headers, List<string[]> rows) {
            var widths = headers
                .Select((header, index) => Math.Max(header.Length, rows.Max(row => row[index].Length)))
                .ToArray();

            string FormatRow(IReadOnlyList<string> values) {
                return string.Join("  ", values.Select((value, index) => value.PadRight(widths[index])).ToArray()).TrimEnd();
            }

            AnsiConsole.MarkupLine(Markup.Escape(FormatRow(headers)));
            AnsiConsole.MarkupLine(new string('-', widths.Sum() + (headers.Count - 1) * 2));
            foreach (var row in rows) {
                AnsiConsole.MarkupLine(Markup.Escape(FormatRow(row)));
            }
        }

        private string FormatOfficeCurrency(decimal usdAmount, string currencyCode) {
            var localAmount = currencyCode.Equals("USD", StringComparison.OrdinalIgnoreCase)
                ? usdAmount
                : _currencyController.ConvertCurrency(currencyCode.ToLowerInvariant(), usdAmount);
            return CurrencyController.FormatPrice(localAmount, currencyCode);
        }

        private static string FormatUsd(decimal amount) {
            return amount.ToString("C", CultureInfo.GetCultureInfo("en-US"));
        }

        private static void Pause() {
            AnsiConsole.MarkupLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }
}
