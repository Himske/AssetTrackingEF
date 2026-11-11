using Microsoft.EntityFrameworkCore;
using AssetTrackingEF.Models;
using AssetTrackingEF.Data;
using AssetTrackingEF.Controllers;
using AssetTrackingEF.Views;

Console.WriteLine("Starting the application...");
Console.WriteLine();

using var context = new AssetDbContext();
var view = new AssetView();
var controller = new AssetController(context);

controller.CreateAsset("Laptop", "Dell", "XPS 15", new DateTime(2021, 6, 15), 1500.00m, "SN123456789");
controller.CreateAsset("Desktop", "Acer", "Aspire TC", new DateTime(2023, 12, 15), 2500.00m, "SN987654321");
controller.CreateAsset("Mobile", "Apple", "iPhone 14", new DateTime(2024, 3, 15), 999.00m, "SN111222333");
controller.CreateAsset("Mobile", "Samsung", "Galaxy S23", new DateTime(2024, 9, 15), 799.00m, "SN444555666");
controller.CreateAsset("Tablet", "Microsoft", "Surface Pro 9", new DateTime(2024, 1, 15), 1200.00m, "SN777888999");
AssetView.DisplayAssets(controller.GetAllAssets());
DeleteAllAssets(controller);

static void DeleteAllAssets(AssetController controller) {
    int deletedCount = controller.DeleteAllAssets();
    Console.WriteLine($"{deletedCount} assets deleted.");
}