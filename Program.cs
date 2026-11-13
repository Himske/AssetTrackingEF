using AssetTrackingEF.Controllers;
using AssetTrackingEF.Data;
using AssetTrackingEF.Models;
using AssetTrackingEF.Views;
using Spectre.Console;
using System.Text;

Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

using var context = new AppDbContext();
var assetController = new AssetController(context);
var currencyController = new CurrencyController(context);
var officeController = new OfficeController(context);
var assetView = new AssetView(assetController, currencyController, officeController);
var officeView = new OfficeView(officeController);
var adminController = new AdminController(context);
var adminView = new AdminView(adminController);
var reportingController = new ReportingController(context);
var reportingView = new ReportingView(reportingController, currencyController);
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
                "Reporting",
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
        case "Reporting":
            reportingView.ShowReportingMenu();
            break;
        case "Exit":
            running = false;
            break;
    }
}