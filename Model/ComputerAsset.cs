using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingEF.Model {
    internal class ComputerAsset : Asset {
        // Laptop or Desktop
        public ComputerAsset(string assetType, string brand, string modelName, DateTime purchaseDate, decimal purchasePrice, string serialNumber) :
            base(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber) {
        }
    }
}
