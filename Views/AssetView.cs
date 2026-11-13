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
        private readonly CurrencyController _currencyController;
        private readonly OfficeController _officeController;

        public AssetView(AssetController controller, CurrencyController currencyController, OfficeController officeController) {
            _controller = controller;
            _currencyController = currencyController;
            _officeController = officeController;
        }

        public void ShowAssetMenu() {
            bool inAssetMenu = true;

            while (inAssetMenu) {
                AnsiConsole.Clear();
                ConsoleHeader.Render();

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an option")
                        .AddChoices([
                            "Add Asset",
                            "Show All Assets",
                            "Search",
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
                        break;
                    case "Search":
                        ShowSearchMenu();
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
            var offices = _officeController.GetAllOffices();
            if (offices.Count == 0) {
                AnsiConsole.MarkupLine("[yellow]No offices found. Add an office before adding an asset.[/]");
                Pause();
                return;
            }

            var office = AnsiConsole.Prompt(
                new SelectionPrompt<Office>()
                    .Title("Select office")
                    .UseConverter(selectedOffice => selectedOffice.Name)
                    .AddChoices(offices));

            _controller.CreateAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber, office.Id);
            AnsiConsole.MarkupLine("[green]Asset added.[/]");
            Pause();
        }

        private void ShowAllAssets() {
            var allAssets = _controller.GetAllAssets();
            var filteredAssets = allAssets;
            var filterDescription = "All Assets";
            bool filtering = true;

            while (filtering) {
                AnsiConsole.Clear();
                ConsoleHeader.Render();
                AnsiConsole.MarkupLine($"[bold cyan]{Markup.Escape(filterDescription)}[/]");
                DisplayAssets(filteredAssets, _currencyController);

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Filter assets")
                        .AddChoices([
                            "Show Only Expired",
                            "Show Only Computers",
                            "Show Only Mobile Devices",
                            "Show Assets by Office",
                            "Show All Assets",
                            "Back"
                        ]));

                switch (choice) {
                    case "Show Only Expired":
                        filteredAssets = allAssets.Where(asset => asset.GetStatus() == "EXPIRED").ToList();
                        filterDescription = "Expired Assets";
                        break;
                    case "Show Only Computers":
                        filteredAssets = allAssets.Where(asset => asset is ComputerAsset).ToList();
                        filterDescription = "Computer Assets";
                        break;
                    case "Show Only Mobile Devices":
                        filteredAssets = allAssets.Where(asset => asset is MobileAsset).ToList();
                        filterDescription = "Mobile Assets";
                        break;
                    case "Show Assets by Office":
                        var offices = _officeController.GetAllOffices();
                        if (offices.Count == 0) {
                            AnsiConsole.MarkupLine("[yellow]No offices found.[/]");
                            Pause();
                            break;
                        }

                        var office = AnsiConsole.Prompt(
                            new SelectionPrompt<Office>()
                                .Title("Select office")
                                .UseConverter(selectedOffice => selectedOffice.Name)
                                .AddChoices(offices));
                        filteredAssets = allAssets.Where(asset => asset.OfficeId == office.Id).ToList();
                        filterDescription = $"Assets at {office.Name}";
                        break;
                    case "Show All Assets":
                        filteredAssets = allAssets;
                        filterDescription = "All Assets";
                        break;
                    case "Back":
                        filtering = false;
                        break;
                }
            }
        }

        private void ShowSearchMenu() {
            bool inSearchMenu = true;
            List<Asset>? searchResults = null;

            while (inSearchMenu) {
                AnsiConsole.Clear();
                ConsoleHeader.Render();
                if (searchResults != null) {
                    ShowSearchResults(searchResults);
                }

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Search assets")
                        .AddChoices([
                            "Search by Brand",
                            "Search by Model",
                            "Search by Office",
                            "Search by Purchase Year",
                            "Back"
                        ]));

                switch (choice) {
                    case "Search by Brand":
                        searchResults = SearchByBrand();
                        break;
                    case "Search by Model":
                        searchResults = SearchByModel();
                        break;
                    case "Search by Office":
                        searchResults = SearchByOffice();
                        break;
                    case "Search by Purchase Year":
                        searchResults = SearchByPurchaseYear();
                        break;
                    case "Back":
                        inSearchMenu = false;
                        break;
                }
            }
        }

        private List<Asset> SearchByBrand() {
            var brand = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter brand to search for:")
                    .Validate(value => !string.IsNullOrWhiteSpace(value)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Brand search term cannot be empty.")));
            return _controller.SearchAssetsByBrand(brand.Trim());
        }

        private List<Asset> SearchByModel() {
            var model = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter model to search for:")
                    .Validate(value => !string.IsNullOrWhiteSpace(value)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Model search term cannot be empty.")));
            return _controller.SearchAssetsByModel(model.Trim());
        }

        private List<Asset> SearchByOffice() {
            var offices = _officeController.GetAllOffices();
            if (offices.Count == 0) {
                return [];
            }

            var office = AnsiConsole.Prompt(
                new SelectionPrompt<Office>()
                    .Title("Select office")
                    .UseConverter(selectedOffice => selectedOffice.Name)
                    .AddChoices(offices));
            return _controller.GetAssetsByOffice(office.Id);
        }

        private List<Asset> SearchByPurchaseYear() {
            var year = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter purchase year:")
                    .Validate(value => value is >= 1 and <= 9999
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Enter a year between 1 and 9999.")));
            return _controller.GetAssetsByPurchaseYear(year);
        }

        private void ShowSearchResults(List<Asset> assets) {
            if (assets.Count == 0) {
                AnsiConsole.MarkupLine("[yellow]No assets were found.[/]");
            }
            else {
                DisplayAssets(assets, _currencyController);
            }
            AnsiConsole.WriteLine();
        }

        private void UpdateAsset() {
            DisplayAssets(_controller.GetAllAssets(), _currencyController);
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
            DisplayAssets(_controller.GetAllAssets(), _currencyController);
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

        public static void DisplayAssets(List<Asset> assets, CurrencyController _currencyController) {
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

                var localPurchasePrice = asset.PurchasePrice;
                if (asset.Office.Currency != "USD") {
                    localPurchasePrice = _currencyController.ConvertCurrency(asset.Office.Currency.ToLower(), asset.PurchasePrice);
                }

                table.AddRow(
                    $"[{color}]{asset.Id}[/]",
                    $"[{color}]{Markup.Escape(asset.AssetType)}[/]",
                    $"[{color}]{Markup.Escape(asset.Brand)}[/]",
                    $"[{color}]{Markup.Escape(asset.ModelName)}[/]",
                    $"[{color}]{asset.PurchaseDate:d}[/]",
                    $"[{color}]{Markup.Escape(CurrencyController.FormatPrice(localPurchasePrice, asset.Office.Currency))}[/]",
                    $"[{color}]{Markup.Escape(asset.SerialNumber)}[/]",
                    $"[{color}]{Markup.Escape(asset.Office.Name)}[/]",
                    $"[{color}]{Markup.Escape(asset.Office.Country)}[/]");
            }

            AnsiConsole.Write(table);
        }
    }
}
