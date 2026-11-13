using Spectre.Console;
using AssetTrackingEF.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingEF.Views {
    internal class AdminView {
        private readonly Controllers.AdminController _controller;
        public AdminView(Controllers.AdminController controller) {
            _controller = controller;
        }
        private void Pause() {
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
        public void ShowAdminMenu() {
            bool inAdmin = true;

            while (inAdmin) {
                AnsiConsole.Clear();
                ConsoleHeader.Render();

                var adminChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Admin")
                        .AddChoices([
                            "Seed Database",
                            "Empty Database",
                            "Refresh Currency Rates",
                            "Back"
                        ]));

                switch (adminChoice) {
                    case "Seed Database":
                        GenerateData.SeedDatabase();
                        Pause();
                        break;
                    case "Empty Database":
                        if (AnsiConsole.Confirm("Empty the database?")) {
                            int deletedAssets = _controller.DeleteAllAssets();
                            int deletedOffices = _controller.DeleteAllOffices();
                            int deletedCurrencyRates = _controller.DeleteAllCurrencyRates();
                            AnsiConsole.MarkupLine($"[green]{deletedAssets} assets deleted.[/]");
                            AnsiConsole.MarkupLine($"[green]{deletedOffices} offices deleted.[/]");
                            AnsiConsole.MarkupLine($"[green]{deletedCurrencyRates} currency rates deleted.[/]");
                        }
                        Pause();
                        break;
                    case "Refresh Currency Rates":
                        _controller.RefreshCurrencyRates();
                        AnsiConsole.MarkupLine("[green]Currency rates refreshed.[/]");
                        Pause();
                        break;
                    case "Back":
                        inAdmin = false;
                        break;
                }
            }
        }
    }
}
