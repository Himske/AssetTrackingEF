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

//GenerateData.SeedDatabase();
AssetView.DisplayAssets(controller.GetAllAssets());
//GenerateData.DeleteAllAssets();
