using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingEF.Models {
    internal class Office {
        public Office(string name, string country, string currency) {
            Name = name;
            Country = country;
            Currency = currency;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public List<Asset> Assets { get; set; } = [];
    }
}
