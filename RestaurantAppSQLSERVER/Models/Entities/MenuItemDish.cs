using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class MenuItemDish
    {
        // Chei straine compuse care formeaza cheia primara a tabelei de legatura
        public int MenuItemId { get; set; }
        public int DishId { get; set; }

        // Cantitatea acestui Dish in cadrul acestui MenuItem
        public int Quantity { get; set; } // Ex: pentru fish&chips, cartofi prajiti 200g

        // Proprietati de navigare catre entitatile legate
        public MenuItem MenuItem { get; set; }
        public Dish Dish { get; set; }
    }
}
