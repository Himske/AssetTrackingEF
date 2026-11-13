using Spectre.Console;

namespace AssetTrackingEF.Views {
    internal static class ConsoleHeader {
        public static void Render() {
            AnsiConsole.Write(new FigletText("Asset Tracker Pro")
                .Centered()
                .Color(Color.Cyan1));
        }
    }
}
