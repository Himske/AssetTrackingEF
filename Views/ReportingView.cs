using AssetTrackingEF.Controllers;
using AssetTrackingEF.Models;
using Spectre.Console;
using System.Globalization;
using System.Text;
using System.Text.Json;

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
                ConsoleHeader.Render();

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
            var textReport = new StringBuilder();
            var csvRows = new List<string[]> {
                new[] { "Record Type", "Office", "Asset ID", "Asset Type", "Brand", "Model", "Purchase Date", "Price", "Status", "Total Office Value", "Currency" }
            };
            var jsonOffices = offices.Select(office => new {
                office.Name,
                office.Currency,
                TotalValueUsd = office.Assets.Sum(asset => asset.PurchasePrice),
                TotalValue = FormatOfficeCurrency(office.Assets.Sum(asset => asset.PurchasePrice), office.Currency),
                Assets = office.Assets.OrderBy(asset => asset.Id).Select(asset => new {
                    asset.Id,
                    asset.AssetType,
                    asset.Brand,
                    asset.ModelName,
                    asset.PurchaseDate,
                    PriceUsd = asset.PurchasePrice,
                    Price = FormatOfficeCurrency(asset.PurchasePrice, office.Currency),
                    Status = asset.GetStatus()
                })
            }).ToList();

            if (offices.Count == 0) {
                AnsiConsole.MarkupLine("[yellow]No offices found.[/]");
                textReport.AppendLine("No offices found.");
            }

            foreach (var office in offices) {
                var heading = $"{office.Name.ToUpperInvariant()} OFFICE";
                WriteReportBanner(heading);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"{"ID",-4} {"Asset",-8} {"Brand",-20} {"Price",-22} Status");
                WriteReportRule('-');

                textReport.AppendLine(heading);
                textReport.AppendLine();
                textReport.AppendLine($"{"ID",-4} {"Asset",-8} {"Brand",-20} {"Price",-22} Status");
                textReport.AppendLine(new string('-', ReportWidth));
                foreach (var asset in office.Assets.OrderBy(asset => asset.Id)) {
                    var status = asset.GetStatus();
                    var color = status switch {
                        "EXPIRED" => "grey",
                        "RED" => "red",
                        "YELLOW" => "yellow",
                        _ => "green"
                    };
                    var price = FormatOfficeCurrency(asset.PurchasePrice, office.Currency);
                    var assetLine = $"{asset.Id,-4} {asset.AssetType,-8} {asset.Brand,-20} {price,-22} {status}";
                    AnsiConsole.MarkupLine(
                        $"{asset.Id,-4} {Markup.Escape(asset.AssetType),-8} {Markup.Escape(asset.Brand),-20} {Markup.Escape(price),-22} [{color}]{status}[/]");
                    textReport.AppendLine(assetLine);
                    csvRows.Add(["Asset", office.Name, asset.Id.ToString(CultureInfo.InvariantCulture), asset.AssetType, asset.Brand, asset.ModelName, asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), price, status, "", office.Currency]);
                }

                var totalUsd = office.Assets.Sum(asset => asset.PurchasePrice);
                var totalValue = FormatOfficeCurrency(totalUsd, office.Currency);
                AnsiConsole.MarkupLine($"\nTotal Office Value: [bold]{Markup.Escape(totalValue)}[/]");
                textReport.AppendLine();
                textReport.AppendLine($"Total Office Value: {totalValue}");
                csvRows.Add(["Office Total", office.Name, "", "", "", "", "", "", "", totalValue, office.Currency]);
                AnsiConsole.WriteLine();
                WriteReportRule('=');
                AnsiConsole.WriteLine();
                textReport.AppendLine();
                textReport.AppendLine(new string('=', ReportWidth));
                textReport.AppendLine();
            }

            PromptReportExport("assets-per-office", textReport.ToString(), BuildCsv(csvRows), new { Report = "Assets and Value per Office", Offices = jsonOffices });
            Pause();
        }

        private void ShowAssetCountsStatusAndHighestPrices() {
            var textReport = new StringBuilder();
            var csvRows = new List<string[]> {
                new[] { "Section", "Office", "Asset Count", "Status", "Asset", "Purchase Date", "Rank", "Purchase Price (USD)" }
            };
            WriteReportBanner("REPORT");
            AnsiConsole.WriteLine();
            textReport.AppendLine("REPORT");
            textReport.AppendLine();

            AnsiConsole.MarkupLine("[bold]Office Asset Counts[/]");
            WriteReportRule('-');
            textReport.AppendLine("Office Asset Counts");
            textReport.AppendLine(new string('-', ReportWidth));
            var offices = _controller.GetOfficesWithAssets();
            var officeNameWidth = offices.Count == 0 ? 0 : offices.Max(office => office.Name.Length);
            foreach (var office in offices) {
                var officeName = office.Name.PadRight(officeNameWidth);
                AnsiConsole.MarkupLine($"{Markup.Escape(officeName)} : {office.Assets.Count}");
                textReport.AppendLine($"{officeName} : {office.Assets.Count}");
                csvRows.Add(["Office Asset Counts", office.Name, office.Assets.Count.ToString(CultureInfo.InvariantCulture), "", "", "", "", ""]);
            }
            if (offices.Count == 0) {
                AnsiConsole.MarkupLine("No offices found.");
                textReport.AppendLine("No offices found.");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Assets Near Or Expired[/]");
            WriteReportRule('-');
            textReport.AppendLine();
            textReport.AppendLine();
            textReport.AppendLine("Assets Near Or Expired");
            textReport.AppendLine(new string('-', ReportWidth));
            var assetsToReview = _controller.GetAssetsCloseToOrExpired();
            List<string[]> statusRows;
            if (assetsToReview.Count == 0) {
                AnsiConsole.MarkupLine("No assets are near expiration or expired.");
                textReport.AppendLine("No assets are near expiration or expired.");
                statusRows = [];
            }
            else {
                statusRows = assetsToReview.Select(asset => new[] {
                    asset.GetStatus(),
                    $"{asset.Brand} {asset.ModelName}",
                    asset.Office.Name,
                    asset.PurchaseDate.ToString("MM/dd/yyyy", CultureInfo.GetCultureInfo("en-US"))
                }).ToList();
                textReport.AppendLine(FormatTextTable(["Status", "Asset", "Office", "Purchase Date"], statusRows));
                WriteTextTable(["Status", "Asset", "Office", "Purchase Date"], statusRows);
                csvRows.AddRange(assetsToReview.Select(asset => new[] {
                    "Assets Near Or Expired", asset.Office.Name, "", asset.GetStatus(), $"{asset.Brand} {asset.ModelName}",
                    asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), "", ""
                }));
            }

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Most Expensive Assets[/]");
            WriteReportRule('-');
            textReport.AppendLine();
            textReport.AppendLine();
            textReport.AppendLine("Most Expensive Assets");
            textReport.AppendLine(new string('-', ReportWidth));
            var mostExpensiveAssets = _controller.GetAssetsByPurchasePrice();
            List<string[]> priceRows;
            if (mostExpensiveAssets.Count == 0) {
                AnsiConsole.MarkupLine("No assets found.");
                textReport.AppendLine("No assets found.");
                priceRows = [];
            }
            else {
                priceRows = mostExpensiveAssets
                    .Select((asset, index) => new[] {
                        (index + 1).ToString(CultureInfo.InvariantCulture),
                        $"{asset.Brand} {asset.ModelName}",
                        asset.Office.Name,
                        FormatUsd(asset.PurchasePrice)
                    })
                    .ToList();
                textReport.AppendLine(FormatTextTable(["Rank", "Asset", "Office", "Purchase Price (USD)"], priceRows));
                WriteTextTable(["Rank", "Asset", "Office", "Purchase Price (USD)"], priceRows);
                csvRows.AddRange(mostExpensiveAssets.Select((asset, index) => new[] {
                    "Most Expensive Assets", asset.Office.Name, "", "", $"{asset.Brand} {asset.ModelName}", "",
                    (index + 1).ToString(CultureInfo.InvariantCulture), FormatUsd(asset.PurchasePrice)
                }));
            }

            AnsiConsole.WriteLine();
            WriteReportRule('=');
            textReport.AppendLine();
            textReport.AppendLine(new string('=', ReportWidth));
            var jsonReport = new {
                Report = "Asset Counts, Status, and Highest Prices",
                OfficeAssetCounts = offices.Select(office => new { Office = office.Name, AssetCount = office.Assets.Count }),
                AssetsNearOrExpired = assetsToReview.Select(asset => new {
                    Status = asset.GetStatus(), Asset = $"{asset.Brand} {asset.ModelName}", Office = asset.Office.Name, asset.PurchaseDate
                }),
                MostExpensiveAssets = mostExpensiveAssets.Select((asset, index) => new {
                    Rank = index + 1, Asset = $"{asset.Brand} {asset.ModelName}", Office = asset.Office.Name, PurchasePriceUsd = asset.PurchasePrice
                })
            };
            PromptReportExport("asset-summary", textReport.ToString(), BuildCsv(csvRows), jsonReport);
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
            foreach (var line in FormatTextTable(headers, rows).Split(Environment.NewLine)) {
                AnsiConsole.MarkupLine(Markup.Escape(line));
            }
        }

        private static string FormatTextTable(IReadOnlyList<string> headers, List<string[]> rows) {
            var widths = headers
                .Select((header, index) => Math.Max(header.Length, rows.Max(row => row[index].Length)))
                .ToArray();

            string FormatRow(IReadOnlyList<string> values) {
                return string.Join("  ", values.Select((value, index) => value.PadRight(widths[index])).ToArray()).TrimEnd();
            }

            return string.Join(Environment.NewLine, new[] { FormatRow(headers), new string('-', ReportWidth) }
                .Concat(rows.Select(row => FormatRow(row))));
        }

        private static string BuildCsv(IEnumerable<string[]> rows) {
            return string.Join(Environment.NewLine, rows.Select(row => string.Join(",", row.Select(EscapeCsvField))));
        }

        private static string EscapeCsvField(string value) {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static void PromptReportExport(string fileNamePrefix, string text, string csv, object json) {
            if (!AnsiConsole.Confirm("Would you like to export this report?")) {
                return;
            }

            var format = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select export format")
                    .AddChoices(["TXT", "CSV", "JSON", "Cancel"]));
            if (format == "Cancel") {
                return;
            }

            var extension = format.ToLowerInvariant();
            var reportsDirectory = Path.Combine(Environment.CurrentDirectory, "Reports");
            Directory.CreateDirectory(reportsDirectory);
            var filePath = Path.Combine(reportsDirectory, $"{fileNamePrefix}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.{extension}");
            var content = format switch {
                "TXT" => text,
                "CSV" => csv,
                "JSON" => JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }),
                _ => throw new InvalidOperationException($"Unsupported report format '{format}'.")
            };
            File.WriteAllText(filePath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            AnsiConsole.MarkupLine($"[green]Report exported to {Markup.Escape(filePath)}[/]");
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
