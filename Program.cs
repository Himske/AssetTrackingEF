using AssetTrackingEF.Models;
using AssetTrackingEF.Data;
using AssetTrackingEF.Controllers;
using AssetTrackingEF.Views;
using Spectre.Console;

using var context = new AssetDbContext();
var controller = new AssetController(context);

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

void AddAsset() {
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

    controller.CreateAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber);
    AnsiConsole.MarkupLine("[green]Asset added.[/]");
    Pause();
}

void ShowAllAssets() {
    AssetView.DisplayAssets(controller.GetAllAssets());
    Pause();
}

void UpdateAsset() {
    ShowAllAssets();
    var id = AnsiConsole.Ask<int>("Enter asset ID to update:");

    Asset asset;
    try {
        asset = controller.GetAssetById(id);
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

    controller.UpdateAsset(id, assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber);
    AnsiConsole.MarkupLine("[green]Asset updated.[/]");
    Pause();
}

void DeleteAsset() {
    ShowAllAssets();
    var id = AnsiConsole.Ask<int>("Enter asset ID to delete:");

    if (!AnsiConsole.Confirm("Are you sure you want to delete this asset?")) {
        return;
    }

    controller.DeleteAsset(id);
    AnsiConsole.MarkupLine("[green]Delete operation completed.[/]");
    Pause();
}

void ShowAdminMenu() {
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
                    int deletedCount = controller.DeleteAllAssets();
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

void Pause() {
    AnsiConsole.MarkupLine("\nPress any key to continue...");
    Console.ReadKey(true);
}
