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
    // Extinsa pentru a include informatii despre cantitate/gramaj si alergeni
    public class DisplayMenuItem : ViewModelBase // Inherit ViewModelBase if needed for future interactions (e.g., adding to cart)
    {
        // Proprietati comune mapate din procedura stocata GetFullMenuDetails
        public int ItemId { get; set; } // Corespunde ItemId din SP
        public string ItemName { get; set; } // Corespunde ItemName din SP
        public decimal ItemPrice { get; set; } // Corespunde ItemPrice din SP
        public string? ItemPhotoPath { get; set; } // Corespunde ItemPhotoPath din SP
        public string ItemType { get; set; } // Corespunde ItemType din SP ("Dish" or "MenuItem")

        // Proprietati noi pentru informatii suplimentare mapate din SP
        public string? QuantityDisplay { get; set; } // Corespunde QuantityDisplay din SP
        public string? AllergensString { get; set; } // Corespunde AllergensString din SP
        public string? MenuItemComponentsString { get; set; } // Corespunde MenuItemComponentsString din SP (NULL pentru Dish)

        // Proprietati pentru maparea CategoryId si CategoryName din SP (utile pentru grupare)
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;


        // Reference to the original entity (optional, might not be needed if all data comes from SP)
        // public Dish? OriginalDish { get; set; } = null;
        // public MenuItem? OriginalMenuItem { get; set; } = null;


        // Constructor gol pentru maparea din rezultatul procedurii stocate
        public DisplayMenuItem()
        {
            // Acest constructor este folosit de EF Core sau alte metode de mapare
            // atunci cand se mapeaza rezultatul unui query/SP la acest tip.
        }

        // Poti pastra constructorii originali daca ii folosesti in alte parti, dar pentru LoadMenuData
        // vei mapa direct din rezultatul SP-ului in obiecte DisplayMenuItem noi.
        /*
        // Constructor pentru un Dish
        public DisplayMenuItem(Dish dish)
        {
            if (dish == null) throw new ArgumentNullException(nameof(dish));

            ItemId = dish.Id;
            ItemName = dish.Name;
            ItemPrice = dish.Price;
            ItemPhotoPath = dish.PhotoPath;
            ItemType = "Dish";
            // OriginalDish = dish; // Daca pastrezi referinta

            QuantityDisplay = $"{dish.Quantity}g";
            // Alergenii si componentele vor fi setate ulterior
            AllergensString = "Incarcare alergeni...";
            MenuItemComponentsString = null;
        }

        // Constructor pentru un MenuItem (Meniu)
        public DisplayMenuItem(MenuItem menuItem)
        {
            if (menuItem == null) throw new ArgumentNullException(nameof(menuItem));

            ItemId = menuItem.Id;
            ItemName = menuItem.Name;
            ItemPrice = menuItem.Price;
            ItemPhotoPath = menuItem.PhotoPath;
            ItemType = "MenuItem";
            // OriginalMenuItem = menuItem; // Daca pastrezi referinta

            QuantityDisplay = "Meniu";
            // Alergenii si componentele vor fi setate ulterior
            AllergensString = "Incarcare alergeni...";
            MenuItemComponentsString = "Incarcare componente...";
        }
        */


        // Override ToString for debugging or simple display
        public override string ToString()
        {
            return $"{ItemName} ({ItemType}) - {ItemPrice:C2}";
        }
    }
}
