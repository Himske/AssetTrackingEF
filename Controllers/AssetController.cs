using System;
using System.Collections.Generic;
using System.Text;
using AssetTrackingEF.Data;
using AssetTrackingEF.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackingEF.Controllers {
    internal class AssetController {
        private readonly AppDbContext _context;

        public AssetController(AppDbContext context) {
            _context = context;
            _context.Database.EnsureCreated(); // Create the database if it doesn't exist
        }

        public void CreateAsset(string assetType, string brand, string modelName, DateTime purchaseDate, decimal purchasePrice, string serialNumber, int officeId) {
            if (new[] { "Desktop", "Laptop" }.Contains(assetType)) {
                _context.Assets.Add(new ComputerAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber, officeId));
                _context.SaveChanges();
            }
            else if (new[] { "Mobile", "Tablet" }.Contains(assetType)) {
                _context.Assets.Add(new MobileAsset(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber, officeId));
                _context.SaveChanges();
            }
            else {
                throw new ArgumentException("Invalid asset type.");
            }
        }

        public List<Asset> GetAllAssets() {
            return [.. _context.Assets.Include(a => a.Office)];
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
    }
}
