using System;
using System.Collections.Generic;
using System.Text;
using AssetTrackingEF.Data;
using AssetTrackingEF.Models;

namespace AssetTrackingEF.Controllers {
    internal class AssetController {
        private readonly AssetDbContext _context;

        public AssetController(AssetDbContext context) {
            _context = context;
            _context.Database.EnsureCreated(); // Create the database if it doesn't exist
        }

        public void CreateAsset(string assetType, string brand, string modelName, DateTime purchaseDate, decimal purchasePrice, string serialNumber) {
            if (new[] { "Desktop", "Laptop" }.Contains(assetType)) {
                _context.Assets.Add(new ComputerAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber));
                _context.SaveChanges();
            }
            else if (new[] { "Mobile", "Tablet" }.Contains(assetType)) {
                _context.Assets.Add(new MobileAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber));
                _context.SaveChanges();
            }
            else {
                throw new ArgumentException("Invalid asset type.");
            }
        }

        public List<Asset> GetAllAssets() {
            return [.. _context.Assets];
        }

        public Asset GetAssetById(int assetId) {
            var asset = _context.Assets.Find(assetId);
            return asset ?? throw new ArgumentException("Asset not found.");
        }

        public void UpdateAsset(int assetId, string assetType, string brand, string modelName, DateTime purchaseDate, decimal purchasePrice, string serialNumber) {
            var asset = _context.Assets.Find(assetId);
            if (asset != null) {
                asset.AssetType = assetType;
                asset.Brand = brand;
                asset.ModelName = modelName;
                asset.PurchaseDate = purchaseDate;
                asset.PurchasePrice = purchasePrice;
                asset.SerialNumber = serialNumber;
                _context.SaveChanges();
            }
        }

        public void DeleteAsset(int assetId) {
            var asset = _context.Assets.Find(assetId);
            if (asset != null) {
                _context.Assets.Remove(asset);
                _context.SaveChanges();
            }
        }

        public int DeleteAllAssets() {
            var assets = _context.Assets.ToList();
            if (assets.Count == 0) {
                return 0;
            }
            _context.Assets.RemoveRange(assets);
            _context.SaveChanges();
            return assets.Count;
        }
    }
}
