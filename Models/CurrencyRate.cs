using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingEF.Models {
    internal class CurrencyRate {
        public int Id { get; set; }
        public required string Currency { get; set; }
        public decimal Rate { get; set; }
    }
}
