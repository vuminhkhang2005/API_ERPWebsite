using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebERP.Models
{
    public class Warehouse
    {
        [Key]
        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; }

        public string Location { get; set; }

        // Navigation
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

        // EF constructor
        private Warehouse() { }

        // Main constructor
        public Warehouse(string name, string location)
        {
            WarehouseName = name;
            Location = location;
        }
    }
}
