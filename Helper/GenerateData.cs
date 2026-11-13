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
        officeController.CreateOffice("Frankfurt", "Germany", "EUR");
        officeController.CreateOffice("Tokyo", "Japan", "JPY");
        officeController.CreateOffice("London", "UK", "GBP");
        officeController.CreateOffice("Helsingborg", "Sweden", "SEK");
        Office office = officeController.GetOfficeByName("Headquarters");
        assetController.CreateAsset("Laptop", "Dell", "XPS 15", new DateTime(2021, 6, 15), 1500.00m, "SN123456789", office.Id);
        assetController.CreateAsset("Desktop", "Acer", "Aspire TC", new DateTime(2023, 12, 15), 2500.00m, "SN987654321", office.Id);
        assetController.CreateAsset("Mobile", "Apple", "iPhone 14", new DateTime(2024, 3, 15), 999.00m, "SN111222333", office.Id);
        assetController.CreateAsset("Mobile", "Samsung", "Galaxy S23", new DateTime(2024, 9, 15), 799.00m, "SN444555666", office.Id);
        assetController.CreateAsset("Tablet", "Microsoft", "Surface Pro 9", new DateTime(2024, 1, 15), 1200.00m, "SN777888999", office.Id);
        office = officeController.GetOfficeByName("Frankfurt");
        assetController.CreateAsset("Laptop", "Lenovo", "ThinkPad X1", new DateTime(2022, 5, 10), 1800.00m, "SN222333444", office.Id);
        assetController.CreateAsset("Desktop", "HP", "Pavilion", new DateTime(2023, 11, 20), 2200.00m, "SN555666777", office.Id);
        assetController.CreateAsset("Mobile", "Google", "Pixel 8", new DateTime(2024, 4, 5), 699.00m, "SN888999000", office.Id);
        office = officeController.GetOfficeByName("Tokyo");
        assetController.CreateAsset("Laptop", "ASUS", "ZenBook 14", new DateTime(2022, 8, 25), 1400.00m, "SN333444555", office.Id);
        assetController.CreateAsset("Desktop", "MSI", "Trident 3", new DateTime(2023, 10, 30), 2000.00m, "SN666777888", office.Id);
        assetController.CreateAsset("Mobile", "Sony", "Xperia 1 IV", new DateTime(2024, 2, 18), 899.00m, "SN999000111", office.Id);
        office = officeController.GetOfficeByName("London");
        assetController.CreateAsset("Laptop", "Razer", "Blade 15", new DateTime(2022, 7, 12), 2200.00m, "SN444555666", office.Id);
        assetController.CreateAsset("Desktop", "Corsair", "One i200", new DateTime(2023, 9, 22), 3000.00m, "SN777888999", office.Id);
        assetController.CreateAsset("Mobile", "OnePlus", "10 Pro", new DateTime(2024, 5, 8), 799.00m, "SN000111222", office.Id);
        office = officeController.GetOfficeByName("Helsingborg");
        assetController.CreateAsset("Laptop", "Acer", "Swift 3", new DateTime(2022, 9, 5), 1000.00m, "SN555666777", office.Id);
        assetController.CreateAsset("Desktop", "Dell", "Inspiron", new DateTime(2023, 8, 15), 1800.00m, "SN888999000", office.Id);
        assetController.CreateAsset("Mobile", "Nokia", "X30", new DateTime(2024, 6, 20), 499.00m, "SN111222333", office.Id);
        Console.WriteLine("Database seeded with initial data.");
    }
}
