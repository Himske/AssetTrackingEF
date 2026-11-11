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
            foreach (var asset in assets) {
                Console.WriteLine($"ID: {asset.Id}, Type: {asset.AssetType}, Brand: {asset.Brand}, Model: {asset.ModelName}, Purchase Date: {asset.PurchaseDate:d}, Price: {asset.PurchasePrice:C}, Serial Number: {asset.SerialNumber}, Status: {asset.GetStatus()}");
            }
        }

        public static void DeleteAssetConfirmation(int assetId) {
            Console.WriteLine($"Asset with ID {assetId} has been deleted.");
        }
    }
}
