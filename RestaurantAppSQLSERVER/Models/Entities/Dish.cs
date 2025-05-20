using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int TotalQuantity { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string? PhotoPath { get; set; }
        public bool IsAvailable { get; set; }
        public string? Description { get; set; }
        public ICollection<DishAllergen> DishAllergens { get; set; } = new List<DishAllergen>();
        public ICollection<MenuItemDish> MenuItemDishes { get; set; } = new List<MenuItemDish>();
    }
}
