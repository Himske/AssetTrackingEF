using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingEF.Controllers {
    internal class AdminController {
        private readonly Data.AppDbContext _context;
        public AdminController(Data.AppDbContext context) {
            _context = context;
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
