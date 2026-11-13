using AssetTrackingEF.Controllers;
using AssetTrackingEF.Models;
using Spectre.Console;
using System.Globalization;

namespace AssetTrackingEF.Views {
    internal class ReportingView {
        private const int ReportWidth = 80;
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
                WriteReportBanner(heading);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"{"ID",-4} {"Asset",-8} {"Brand",-20} {"Price",-22} Status");
                WriteReportRule('-');

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
                        $"{asset.Id,-4} {Markup.Escape(asset.AssetType),-8} {Markup.Escape(asset.Brand),-20} {Markup.Escape(price),-22} [{color}]{status}[/]");
                }

                var totalValue = FormatOfficeCurrency(office.Assets.Sum(asset => asset.PurchasePrice), office.Currency);
                AnsiConsole.MarkupLine($"\nTotal Office Value: [bold]{Markup.Escape(totalValue)}[/]");
                AnsiConsole.WriteLine();
                WriteReportRule('=');
                AnsiConsole.WriteLine();
            }

            Pause();
        }

        private void ShowAssetCountsStatusAndHighestPrices() {
            WriteReportBanner("REPORT");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[bold]Office Asset Counts[/]");
            WriteReportRule('-');
            var offices = _controller.GetOfficesWithAssets();
            var officeNameWidth = offices.Count == 0 ? 0 : offices.Max(office => office.Name.Length);
            foreach (var office in offices) {
                var officeName = office.Name.PadRight(officeNameWidth);
                AnsiConsole.MarkupLine($"{Markup.Escape(officeName)} : {office.Assets.Count}");
            }
            if (offices.Count == 0) {
                AnsiConsole.MarkupLine("No offices found.");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Assets Near Or Expired[/]");
            WriteReportRule('-');
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
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Most Expensive Assets[/]");
            WriteReportRule('-');
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
            WriteReportRule('=');
            Pause();
        }

        private static void WriteReportRule(char character) {
            AnsiConsole.MarkupLine(new string(character, ReportWidth));
        }

        private static void WriteReportBanner(string title) {
            var maximumTitleLength = ReportWidth - 4;
            if (title.Length > maximumTitleLength) {
                title = title[..maximumTitleLength];
            }

            var bannerTitle = $" {title} ";
            var leftPadding = (ReportWidth - bannerTitle.Length) / 2;
            var rightPadding = ReportWidth - bannerTitle.Length - leftPadding;
            AnsiConsole.MarkupLine($"[bold cyan]{new string('=', leftPadding)}{Markup.Escape(bannerTitle)}{new string('=', rightPadding)}[/]");
        }

        private static void WriteTextTable(IReadOnlyList<string> headers, List<string[]> rows) {
            var widths = headers
                .Select((header, index) => Math.Max(header.Length, rows.Max(row => row[index].Length)))
                .ToArray();

            string FormatRow(IReadOnlyList<string> values) {
                return string.Join("  ", values.Select((value, index) => value.PadRight(widths[index])).ToArray()).TrimEnd();
            }

            AnsiConsole.MarkupLine(Markup.Escape(FormatRow(headers)));
            WriteReportRule('-');
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
