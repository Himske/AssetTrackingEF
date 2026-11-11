using AssetTrackingEF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingEF.Views {
    internal class AssetView {
        public static void DisplayAssets(List<Asset> assets) {
            if (assets.Count == 0) {
                Console.WriteLine("No assets found.");
                return;
            }
            Console.WriteLine($"{"=".PadLeft(40, '=')} {"Asset List"} {"=".PadRight(40, '=')}");
            Console.WriteLine();
            Console.WriteLine($"{"ID",-5} {"Type",-10} {"Brand",-10} {"Model",-15} {"Purchase Date",-15} {"Price",-15} {"Serial Number",-15}");
            Console.WriteLine(new string('-', 95));
            foreach (var asset in assets) {
                switch (asset.GetStatus()) {
                    case "EXPIRED":
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case "RED":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case "YELLOW":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case "NORMAL":
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
                Console.WriteLine($"{asset.Id,-5} {asset.AssetType,-10} {asset.Brand,-10} {asset.ModelName,-15} {asset.PurchaseDate,-15:d} {asset.PurchasePrice,-15:C} {asset.SerialNumber,-15}");
                Console.ResetColor();
            }
        }

        public static void DeleteAssetConfirmation(int assetId) {
            Console.WriteLine($"Asset with ID {assetId} has been deleted.");
        }
    }
}
