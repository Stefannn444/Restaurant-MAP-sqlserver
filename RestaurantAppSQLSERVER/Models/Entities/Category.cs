using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Descriere optionala

        // Proprietati de navigare catre preparatele si meniurile din aceasta categorie
        public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
