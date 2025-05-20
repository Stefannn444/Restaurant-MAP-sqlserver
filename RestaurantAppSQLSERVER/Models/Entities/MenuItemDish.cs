using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class MenuItemDish
    {
        public int MenuItemId { get; set; }
        public int DishId { get; set; }
        public int Quantity { get; set; }
        public MenuItem MenuItem { get; set; }
        public Dish Dish { get; set; }
    }
}
