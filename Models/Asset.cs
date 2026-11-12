using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingEF.Models {
    abstract class Asset {
        protected Asset(string assetType, string brand, string modelName, DateTime purchaseDate, decimal purchasePrice, string serialNumber, int officeId) {
            AssetType = assetType;
            Brand = brand;
            ModelName = modelName;
            PurchaseDate = purchaseDate;
            PurchasePrice = purchasePrice;
            SerialNumber = serialNumber;
            OfficeId = officeId;
        }

        public int Id { get; set; }
        public string AssetType { get; set; } // Computer or Mobile
        public string Brand { get; set; }
        public string ModelName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; } // In USD
        public string SerialNumber { get; set; }
        public int OfficeId { get; set; } // Foreign key for the related Office
        public Office Office { get; set; } = null!; // Navigation property for the related Office

        public string GetStatus() {
            // Assets "expire" after 3 years.
            DateTime expirationDate = DateTime.Today.AddYears(-3);
            if (PurchaseDate > expirationDate) {
                if (PurchaseDate.AddMonths(-3) < expirationDate) {
                    return "RED";
                }
                else if (PurchaseDate.AddMonths(-6) < expirationDate) {
                    return "YELLOW";
                }
            }
            else {
                return "EXPIRED";
            }
            return "NORMAL";
        }
    }
}
