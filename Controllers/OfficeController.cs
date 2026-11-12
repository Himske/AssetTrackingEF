using System;
using System.Collections.Generic;
using System.Text;
using AssetTrackingEF.Models;

namespace AssetTrackingEF.Controllers {
    internal class OfficeController {
        private readonly Data.AppDbContext _context;

        public OfficeController(Data.AppDbContext context) {
            _context = context;
        }

        public void CreateOffice(string name, string country, string currency) {
            var office = new Office(name, country, currency);
            _context.Offices.Add(office);
            _context.SaveChanges();
        }

        public List<Office> GetAllOffices() {
            return [.. _context.Offices];
        }

        public Office GetOfficeByName(string name) {
            var office = _context.Offices.FirstOrDefault(o => o.Name == name);
            return office ?? throw new ArgumentException("Office not found.");
        }

        public void UpdateOfficeName(int officeId, string newName) {
            var office = _context.Offices.Find(officeId);
            if (office != null) {
                office.Name = newName;
                _context.SaveChanges();
            }
        }

        public void DeleteOffice(int officeId) {
            var office = _context.Offices.Find(officeId);
            if (office != null) {
                _context.Offices.Remove(office);
                _context.SaveChanges();
            }
        }
    }
}
