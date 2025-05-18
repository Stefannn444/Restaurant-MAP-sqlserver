using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class DishAllergen
    {
        // Chei straine compuse care formeaza cheia primara a tabelei de legatura
        public int DishId { get; set; }
        public int AllergenId { get; set; }

        // Proprietati de navigare catre entitatile legate
        public Dish Dish { get; set; }
        public Allergen Allergen { get; set; }
    }
}
