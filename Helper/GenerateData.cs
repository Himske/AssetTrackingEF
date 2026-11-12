using AssetTrackingEF.Controllers;
using AssetTrackingEF.Data;
using AssetTrackingEF.Models;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Helper class for generating simple test data without external libraries.
/// </summary>
public static class GenerateData {
    public static void SeedDatabase() {
        using var context = new AppDbContext();
        var assetController = new AssetController(context);
        var officeController = new OfficeController(context);
        // Check if the database is empty before seeding
        if (context.Assets.Any()) {
            Console.WriteLine("Database already seeded.");
            return;
        }
        officeController.CreateOffice("Headquarters", "USA", "USD");
        Office office = officeController.GetOfficeByName("Headquarters");
        assetController.CreateAsset("Laptop", "Dell", "XPS 15", new DateTime(2021, 6, 15), 1500.00m, "SN123456789", office.Id);
        assetController.CreateAsset("Desktop", "Acer", "Aspire TC", new DateTime(2023, 12, 15), 2500.00m, "SN987654321", office.Id);
        assetController.CreateAsset("Mobile", "Apple", "iPhone 14", new DateTime(2024, 3, 15), 999.00m, "SN111222333", office.Id);
        assetController.CreateAsset("Mobile", "Samsung", "Galaxy S23", new DateTime(2024, 9, 15), 799.00m, "SN444555666", office.Id);
        assetController.CreateAsset("Tablet", "Microsoft", "Surface Pro 9", new DateTime(2024, 1, 15), 1200.00m, "SN777888999", office.Id);
        Console.WriteLine("Database seeded with initial data.");
    }
}
