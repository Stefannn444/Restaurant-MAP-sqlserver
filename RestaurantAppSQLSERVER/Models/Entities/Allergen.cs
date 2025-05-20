using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class Allergen
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<DishAllergen> DishAllergens { get; set; } = new List<DishAllergen>();
    }
}
