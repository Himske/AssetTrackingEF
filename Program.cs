using AssetTrackingEF.Controllers;
using AssetTrackingEF.Data;
using AssetTrackingEF.Models;
using AssetTrackingEF.Views;
using Spectre.Console;

using var context = new AppDbContext();
var assetController = new AssetController(context);
var assetView = new AssetView(assetController);
var officeController = new OfficeController(context);
var officeView = new OfficeView(officeController);
var adminController = new AdminController(context);
var adminView = new AdminView(adminController);
bool running = true;

while (running) {
    AnsiConsole.Clear();
    AnsiConsole.MarkupLine("[bold cyan]Asset Tracking[/]");

    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select an option")
            .AddChoices([
                "Asset",
                "Office",
                "Admin",
                "Exit"
            ]));

    switch (choice) {
        case "Asset":
            assetView.ShowAssetMenu();
            break;
        case "Office":
            officeView.ShowOfficeMenu();
            break;
        case "Admin":
            adminView.ShowAdminMenu();
            break;
        case "Exit":
            running = false;
            break;
    }
}