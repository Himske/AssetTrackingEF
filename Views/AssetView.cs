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

        public void Run() {
            bool running = true;

            while (running) {
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
                            "Admin",
                            "Exit"
                        ]));

                switch (choice) {
                    case "Add Asset":
                        AddAsset();
                        break;
                    case "Show All Assets":
                        ShowAllAssets();
                        break;
                    case "Update Asset":
                        UpdateAsset();
                        break;
                    case "Delete Asset":
                        DeleteAsset();
                        break;
                    case "Admin":
                        ShowAdminMenu();
                        break;
                    case "Exit":
                        running = false;
                        break;
                }
            }
        }

        private void AddAsset() {
            var assetType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select asset type")
                    .AddChoices(["Laptop", "Desktop", "Mobile", "Tablet"]));

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

            _controller.CreateAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber);
            AnsiConsole.MarkupLine("[green]Asset added.[/]");
            Pause();
        }

        private void ShowAllAssets() {
            DisplayAssets(_controller.GetAllAssets());
            Pause();
        }

        private void UpdateAsset() {
            ShowAllAssets();
            var id = AnsiConsole.Ask<int>("Enter asset ID to update:");

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
            var id = AnsiConsole.Ask<int>("Enter asset ID to delete:");

            if (!AnsiConsole.Confirm("Are you sure you want to delete this asset?")) {
                return;
            }

            _controller.DeleteAsset(id);
            AnsiConsole.MarkupLine("[green]Delete operation completed.[/]");
            Pause();
        }

        private void ShowAdminMenu() {
            bool inAdmin = true;

            while (inAdmin) {
                AnsiConsole.Clear();
                var adminChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Admin")
                        .AddChoices([
                            "Seed Database",
                            "Delete All Assets",
                            "Back"
                        ]));

                switch (adminChoice) {
                    case "Seed Database":
                        GenerateData.SeedDatabase();
                        Pause();
                        break;
                    case "Delete All Assets":
                        if (AnsiConsole.Confirm("Delete all assets?")) {
                            int deletedCount = _controller.DeleteAllAssets();
                            AnsiConsole.MarkupLine($"[green]{deletedCount} assets deleted.[/]");
                        }
                        Pause();
                        break;
                    case "Back":
                        inAdmin = false;
                        break;
                }
            }
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
                    $"[{color}]{Markup.Escape(asset.SerialNumber)}[/]");
            }

            AnsiConsole.Write(table);
        }
    }
}
