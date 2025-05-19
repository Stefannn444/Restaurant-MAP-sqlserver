using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.ViewModels; // Assuming ViewModelBase is here
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Wrappers
{
    // Wrapper class to represent either a Dish or a MenuItem for display in the client menu
    public class DisplayMenuItem : ViewModelBase // Inherit ViewModelBase if needed for future interactions (e.g., adding to cart)
    {
        // Properties to hold data common to Dish and MenuItem (or accessed from them)
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? PhotoPath { get; set; }
        public string ItemType { get; set; } // "Dish" or "MenuItem" to distinguish

        // Reference to the original entity (optional, but useful)
        public Dish OriginalDish { get; set; } = null;
        public MenuItem OriginalMenuItem { get; set; } = null;


        // Constructor for a Dish
        public DisplayMenuItem(Dish dish)
        {
            if (dish == null) throw new ArgumentNullException(nameof(dish));

            Id = dish.Id;
            Name = dish.Name;
            Price = dish.Price;
            PhotoPath = dish.PhotoPath;
            ItemType = "Dish";
            OriginalDish = dish;
        }

        // Constructor for a MenuItem (Meniu)
        public DisplayMenuItem(MenuItem menuItem)
        {
            if (menuItem == null) throw new ArgumentNullException(nameof(menuItem));

            Id = menuItem.Id;
            Name = menuItem.Name;
            Price = menuItem.Price;
            PhotoPath = menuItem.PhotoPath;
            ItemType = "MenuItem"; // Use "MenuItem" to distinguish from "Dish"
            OriginalMenuItem = menuItem;
        }

        // You could add other properties here, like:
        // public string Description { get; set; } // If you add Description to MenuItem later
        // public string AllergensInfo { get; set; } // If you want to display allergen info here
        // public ICommand AddToCartCommand { get; set; } // If you add add-to-cart functionality later

        // Override ToString for debugging or simple display
        public override string ToString()
        {
            return $"{Name} ({ItemType}) - {Price:C2}";
        }
    }
}
