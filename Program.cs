using Microsoft.EntityFrameworkCore;
using AssetTrackingEF.Model;

Console.WriteLine("Starting the application...");
Console.WriteLine();

using var context = new AssetDbContext();

context.Assets.Add(CreateAsset("Laptop", "Dell", "XPS 15", new DateTime(2021, 6, 15), 1500.00m, "SN123456789"));
context.Assets.Add(CreateAsset("Desktop Computer", "Acer", "Aspire TC", new DateTime(2023, 12, 15), 2500.00m, "SN987654321"));
context.Assets.Add(CreateAsset("iPhone", "Apple", "iPhone 14", new DateTime(2024, 3, 15), 999.00m, "SN111222333"));
context.Assets.Add(CreateAsset("Samsung", "Samsung", "Galaxy S23", new DateTime(2024, 9, 15), 799.00m, "SN444555666"));
context.SaveChanges();
ShowAllAssets(context);
DeleteAllAssets(context);

static Asset CreateAsset(string assetType, string brand, string modelName, DateTime purchaseDate, decimal purchasePrice, string serialNumber) {
    if (new[] { "Desktop Computer", "Laptop" }.Contains(assetType)) {
        return new ComputerAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber);
    }
    else if (new[] { "iPhone", "Samsung", "Nokia", "Tablet" }.Contains(assetType)) {
        return new MobileAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber);
    }
    else {
        throw new ArgumentException("Invalid asset type.");
    }
}

static void ShowAllAssets(AssetDbContext context) {
    var assets = context.Assets.ToList();
    if (assets.Count == 0) {
        Console.WriteLine("No assets found.");
        return;
    }
    foreach (var asset in assets) {
        Console.WriteLine($"ID: {asset.Id}, Type: {asset.AssetType}, Brand: {asset.Brand}, Model: {asset.ModelName}, Purchase Date: {asset.PurchaseDate:d}, Price: {asset.PurchasePrice:C}, Serial Number: {asset.SerialNumber}, Status: {asset.GetStatus()}");
    }
}

static void DeleteAllAssets(AssetDbContext context) {
    var assets = context.Assets.ToList();
    if (assets.Count == 0) {
        Console.WriteLine("No assets to delete.");
        return;
    }
    context.Assets.RemoveRange(assets);
    context.SaveChanges();
    Console.WriteLine("All assets deleted.");
}