using AssetTrackingEF.Controllers;
using AssetTrackingEF.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AssetTrackingEF.Views {
    internal class AssetView {
        private readonly AssetController _controller;

        public AssetView(AssetController controller) {
            _controller = controller;
        }

        public void ShowAssetMenu() {
            bool inAssetMenu = true;

            while (inAssetMenu) {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[bold cyan]Asset Tracking[/]");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an option")
                        .AddChoices([
                            "Add Asset",
                            "Show All Assets",
                            "Update Asset",
                            "Delete Asset",
                            "Back"
                        ]));

                switch (choice) {
                    case "Add Asset":
                        AddAsset();
                        break;
                    case "Show All Assets":
                        ShowAllAssets();
                        Pause();
                        break;
                    case "Update Asset":
                        UpdateAsset();
                        break;
                    case "Delete Asset":
                        DeleteAsset();
                        break;
                    case "Back":
                        inAssetMenu = false;
                        break;
                }
            }
        }

        private void AddAsset() {
            var assetType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select asset type")
                    .AddChoices(["Laptop", "Desktop", "Mobile", "Tablet", "Cancel"]));

            if (assetType == "Cancel") {
                AnsiConsole.MarkupLine("[yellow]Add asset canceled.[/]");
                Pause();
                return;
            }

            var brand = AnsiConsole.Ask<string>("Brand:");
            var modelName = AnsiConsole.Ask<string>("Model:");
            var purchaseDate = AnsiConsole.Prompt(
                new TextPrompt<DateTime>("Purchase date (yyyy-mm-dd):"));
            var purchasePrice = AnsiConsole.Prompt(
                new TextPrompt<decimal>("Purchase price (USD):")
                    .Validate(value => value >= 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Price must be 0 or greater.")));
            var serialNumber = AnsiConsole.Ask<string>("Serial number:");
            var officeId = AnsiConsole.Prompt(
                new TextPrompt<int>("Office ID:")
                    .Validate(value => value > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Office ID must be greater than 0.")));

            _controller.CreateAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber, officeId);
            AnsiConsole.MarkupLine("[green]Asset added.[/]");
            Pause();
        }

        private void ShowAllAssets() {
            DisplayAssets(_controller.GetAllAssets());
        }

        private void UpdateAsset() {
            ShowAllAssets();
            var idInput = AnsiConsole.Ask<string>("Enter asset ID to update ([grey]or X to cancel[/]):");
            if (idInput.Equals("x", StringComparison.OrdinalIgnoreCase)) {
                AnsiConsole.MarkupLine("[yellow]Update canceled.[/]");
                Pause();
                return;
            }

            if (!int.TryParse(idInput, out var id)) {
                AnsiConsole.MarkupLine("[red]Invalid asset ID.[/]");
                Pause();
                return;
            }

            Asset asset;
            try {
                asset = _controller.GetAssetById(id);
            }
            catch (ArgumentException) {
                AnsiConsole.MarkupLine("[red]Asset not found.[/]");
                Pause();
                return;
            }

            var assetType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select asset type")
                    .AddChoices(["Laptop", "Desktop", "Mobile", "Tablet"]));

            var brand = AnsiConsole.Ask<string>($"Brand ({asset.Brand}):", asset.Brand);
            var modelName = AnsiConsole.Ask<string>($"Model ({asset.ModelName}):", asset.ModelName);
            var purchaseDate = AnsiConsole.Prompt(
                new TextPrompt<DateTime>($"Purchase date ({asset.PurchaseDate:yyyy-MM-dd}):")
                    .DefaultValue(asset.PurchaseDate));
            var purchasePrice = AnsiConsole.Prompt(
                new TextPrompt<decimal>($"Purchase price ({asset.PurchasePrice}):")
                    .DefaultValue(asset.PurchasePrice)
                    .Validate(value => value >= 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Price must be 0 or greater.")));
            var serialNumber = AnsiConsole.Ask<string>($"Serial number ({asset.SerialNumber}):", asset.SerialNumber);

            _controller.UpdateAsset(id, assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber);
            AnsiConsole.MarkupLine("[green]Asset updated.[/]");
            Pause();
        }

        private void DeleteAsset() {
            ShowAllAssets();
            var idInput = AnsiConsole.Ask<string>("Enter asset ID to delete ([grey]or X to cancel[/]):");
            if (idInput.Equals("x", StringComparison.OrdinalIgnoreCase)) {
                AnsiConsole.MarkupLine("[yellow]Delete canceled.[/]");
                Pause();
                return;
            }

            if (!int.TryParse(idInput, out var id)) {
                AnsiConsole.MarkupLine("[red]Invalid asset ID.[/]");
                Pause();
                return;
            }

            if (!AnsiConsole.Confirm("Are you sure you want to delete this asset?")) {
                return;
            }

            _controller.DeleteAsset(id);
            AnsiConsole.MarkupLine("[green]Delete operation completed.[/]");
            Pause();
        }

        private static void Pause() {
            AnsiConsole.MarkupLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        public static void DisplayAssets(List<Asset> assets) {
            if (assets.Count == 0) {
                AnsiConsole.MarkupLine("[yellow]No assets found.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded).Title("[bold]Asset List[/]");
            table.AddColumn("ID");
            table.AddColumn("Type");
            table.AddColumn("Brand");
            table.AddColumn("Model");
            table.AddColumn("Purchase Date");
            table.AddColumn("Price");
            table.AddColumn("Serial Number");
            table.AddColumn("Office");
            table.AddColumn("Country");

            var orderedAssets = assets
                .OrderBy(asset => asset.AssetType)
                .ThenBy(asset => asset.PurchaseDate);

            foreach (var asset in orderedAssets) {
                var color = asset.GetStatus() switch {
                    "EXPIRED" => "grey",
                    "RED" => "red",
                    "YELLOW" => "yellow",
                    _ => "white"
                };

                table.AddRow(
                    $"[{color}]{asset.Id}[/]",
                    $"[{color}]{Markup.Escape(asset.AssetType)}[/]",
                    $"[{color}]{Markup.Escape(asset.Brand)}[/]",
                    $"[{color}]{Markup.Escape(asset.ModelName)}[/]",
                    $"[{color}]{asset.PurchaseDate:d}[/]",
                    $"[{color}]{asset.PurchasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))}[/]",
                    $"[{color}]{Markup.Escape(asset.SerialNumber)}[/]",
                    $"[{color}]{asset.Office.Name}[/]",
                    $"[{color}]{asset.Office.Country}[/]");
            }

            AnsiConsole.Write(table);
        }
    }
}
