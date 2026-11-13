using System;
using System.Collections.Generic;
using System.Text;
using Spectre.Console;

namespace AssetTrackingEF.Views {
    internal class OfficeView {
        private readonly Controllers.OfficeController _controller;

        public OfficeView(Controllers.OfficeController controller) {
            _controller = controller;
        }

        public void ShowOfficeMenu() {
            bool inOfficeMenu = true;

            while (inOfficeMenu) {
                AnsiConsole.Clear();
                ConsoleHeader.Render();

                var officeChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Office")
                        .AddChoices([
                            "Add Office",
                            "Show All Offices",
                            "Update Office",
                            "Delete Office",
                            "Back"
                        ]));

                switch (officeChoice) {
                    case "Add Office":
                        AddOffice();
                        break;
                    case "Show All Offices":
                        ShowAllOffices();
                        break;
                    case "Update Office":
                        UpdateOffice();
                        break;
                    case "Delete Office":
                        DeleteOffice();
                        break;
                    case "Back":
                        inOfficeMenu = false;
                        break;
                }
            }
        }

        private void AddOffice() {
            var name = AnsiConsole.Ask<string>("Office name ([grey]or X to cancel[/]):");
            if (name.Equals("x", StringComparison.OrdinalIgnoreCase)) {
                AnsiConsole.MarkupLine("[yellow]Add office canceled.[/]");
                Pause();
                return;
            }

            var country = AnsiConsole.Ask<string>("Country:");
            var currency = AnsiConsole.Ask<string>("Currency (e.g. USD, SEK, EUR):");

            _controller.CreateOffice(name, country, currency);
            AnsiConsole.MarkupLine("[green]Office added.[/]");
            Pause();
        }

        private void ShowAllOffices() {
            var offices = _controller.GetAllOffices();

            if (offices.Count == 0) {
                AnsiConsole.MarkupLine("[yellow]No offices found.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded).Title("[bold]Office List[/]");
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Country");
            table.AddColumn("Currency");

            foreach (var office in offices) {
                table.AddRow(
                    office.Id.ToString(),
                    Markup.Escape(office.Name),
                    Markup.Escape(office.Country),
                    Markup.Escape(office.Currency));
            }

            AnsiConsole.Write(table);
            Pause();
        }

        private void UpdateOffice() {
            ShowAllOffices();

            var idInput = AnsiConsole.Ask<string>("Enter office ID to update ([grey]or X to cancel[/]):");
            if (idInput.Equals("x", StringComparison.OrdinalIgnoreCase)) {
                AnsiConsole.MarkupLine("[yellow]Update canceled.[/]");
                Pause();
                return;
            }

            if (!int.TryParse(idInput, out var officeId)) {
                AnsiConsole.MarkupLine("[red]Invalid office ID.[/]");
                Pause();
                return;
            }

            var office = _controller.GetAllOffices().FirstOrDefault(o => o.Id == officeId);
            if (office == null) {
                AnsiConsole.MarkupLine("[red]Office not found.[/]");
                Pause();
                return;
            }

            var newName = AnsiConsole.Ask<string>($"New office name ({office.Name}):", office.Name);
            _controller.UpdateOfficeName(officeId, newName);
            AnsiConsole.MarkupLine("[green]Office updated.[/]");
            Pause();
        }

        private void DeleteOffice() {
            ShowAllOffices();

            var idInput = AnsiConsole.Ask<string>("Enter office ID to delete ([grey]or X to cancel[/]):");
            if (idInput.Equals("x", StringComparison.OrdinalIgnoreCase)) {
                AnsiConsole.MarkupLine("[yellow]Delete canceled.[/]");
                Pause();
                return;
            }

            if (!int.TryParse(idInput, out var officeId)) {
                AnsiConsole.MarkupLine("[red]Invalid office ID.[/]");
                Pause();
                return;
            }

            if (!AnsiConsole.Confirm("Are you sure you want to delete this office?")) {
                return;
            }

            _controller.DeleteOffice(officeId);
            AnsiConsole.MarkupLine("[green]Delete operation completed.[/]");
            Pause();
        }

        private static void Pause() {
            AnsiConsole.MarkupLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }
}
