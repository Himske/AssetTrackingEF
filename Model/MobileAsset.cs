using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingEF.Model {
    internal class MobileAsset : Asset {
        // Smartphone or Tablet
        public MobileAsset(string assetType, string brand, string modelName, DateTime purchaseDate, decimal purchasePrice, string serialNumber) :
            base(assetType, brand, modelName, purchaseDate, purchasePrice, serialNumber) {
        }
    }
}
