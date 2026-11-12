using AssetTrackingEF.Models;
using AssetTrackingEF.Data;
using AssetTrackingEF.Controllers;
using AssetTrackingEF.Views;

using var context = new AssetDbContext();
var controller = new AssetController(context);

var view = new AssetView(controller);
view.Run();
