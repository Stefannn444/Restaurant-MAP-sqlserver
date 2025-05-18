using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class Dish
    {
        public int Id { get; set; } // Cheia primara
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; } // Pret per portie individuala
        public int Quantity { get; set; } // Cantitate per portie individuala (ex: 300g)
        public int TotalQuantity { get; set; } // Cantitate totala disponibila in restaurant (in unitatea Quantity, ex: grame)

        // Cheie straina catre Categorie
        public int CategoryId { get; set; }
        // Proprietate de navigare catre Categorie
        public Category Category { get; set; }

        public string? PhotoPath { get; set; } // Calea catre fotografie (optional)
        public bool IsAvailable { get; set; } // Disponibilitatea preparatului (deriva din TotalQuantity)
        public string? Description { get; set; } // Descriere optionala

        // Proprietate de navigare pentru relatia Many-to-Many cu Allergen (prin tabela de legatura DishAllergen)
        public ICollection<DishAllergen> DishAllergens { get; set; } = new List<DishAllergen>();

        // Proprietate de navigare pentru relatia cu Meniurile (prin tabela de legatura MenuItemDish)
        public ICollection<MenuItemDish> MenuItemDishes { get; set; } = new List<MenuItemDish>();
    }
}
