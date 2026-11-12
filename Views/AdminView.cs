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
                AnsiConsole.MarkupLine("[bold cyan]Asset Tracking[/]");

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
    }
}
