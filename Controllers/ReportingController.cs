using AssetTrackingEF.Data;
using AssetTrackingEF.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackingEF.Controllers {
    internal class ReportingController {
        private readonly AppDbContext _context;

        public ReportingController(AppDbContext context) {
            _context = context;
        }

        public List<Office> GetOfficesWithAssets() {
            return [.. _context.Offices
                .Include(office => office.Assets)
                .OrderBy(office => office.Name)];
        }

        public List<Asset> GetAssetsCloseToOrExpired() {
            return [.. _context.Assets
                .Include(asset => asset.Office)
                .AsEnumerable()
                .Where(asset => asset.GetStatus() is "RED" or "YELLOW" or "EXPIRED")
                .OrderBy(asset => asset.PurchaseDate)];
        }

        public List<Asset> GetAssetsByPurchasePrice() {
            return [.. _context.Assets
                .Include(asset => asset.Office)
                .OrderByDescending(asset => asset.PurchasePrice)
                .ThenBy(asset => asset.AssetType)
                .ThenBy(asset => asset.ModelName)];
        }
    }
}
