using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Wrappers
{
    public class DisplayMenuItem : ViewModelBase
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal ItemPrice { get; set; }
        public string? ItemPhotoPath { get; set; }
        public string ItemType { get; set; }
        public string? QuantityDisplay { get; set; }
        public string? AllergensString { get; set; }
        public string? MenuItemComponentsString { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DisplayMenuItem()
        {
        }
        public override string ToString()
        {
            return $"{ItemName} ({ItemType}) - {ItemPrice:C2}";
        }
    }
}
