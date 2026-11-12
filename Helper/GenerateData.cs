using AssetTrackingEF.Controllers;
using AssetTrackingEF.Data;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Helper class for generating simple test data without external libraries.
/// </summary>
public static class GenerateData {
    public static void SeedDatabase() {
        using var context = new AssetDbContext();
        var controller = new AssetController(context);
        // Check if the database is empty before seeding
        if (context.Assets.Any()) {
            Console.WriteLine("Database already seeded.");
            return;
        }
        controller.CreateAsset("Laptop", "Dell", "XPS 15", new DateTime(2021, 6, 15), 1500.00m, "SN123456789");
        controller.CreateAsset("Desktop", "Acer", "Aspire TC", new DateTime(2023, 12, 15), 2500.00m, "SN987654321");
        controller.CreateAsset("Mobile", "Apple", "iPhone 14", new DateTime(2024, 3, 15), 999.00m, "SN111222333");
        controller.CreateAsset("Mobile", "Samsung", "Galaxy S23", new DateTime(2024, 9, 15), 799.00m, "SN444555666");
        controller.CreateAsset("Tablet", "Microsoft", "Surface Pro 9", new DateTime(2024, 1, 15), 1200.00m, "SN777888999");
        Console.WriteLine("Database seeded with initial data.");
    }

    static void DeleteAllAssets() {
        using var context = new AssetDbContext();
        var controller = new AssetController(context);
        int deletedCount = controller.DeleteAllAssets();
        Console.WriteLine($"{deletedCount} assets deleted.");
    }
}
