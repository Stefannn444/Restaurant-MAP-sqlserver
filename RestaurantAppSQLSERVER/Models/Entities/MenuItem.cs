using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class MenuItem
    {
        public int Id { get; set; } // Cheia primara
        public string Name { get; set; } = string.Empty;

        // Pretul Meniului va fi calculat pe baza preparatelor componente, a cantitatilor lor specifice
        // si a reducerii aplicate meniurilor (din fisierul de configurare).
        // Il stocam aici, dar retine ca ar trebui sa fie actualizat la modificarea preparatelor componente,
        // a cantitatilor lor in meniu sau a procentului de reducere pentru meniuri.
        public decimal Price { get; set; } // Pretul Meniului (calculat)

        // Cheie straina catre Categorie
        public int CategoryId { get; set; }
        // Proprietate de navigare catre Categorie
        public Category Category { get; set; }

        public string? PhotoPath { get; set; } = string.Empty; // Calea catre fotografie (optional)

        // Colectie de preparate care compun acest meniu (prin tabela de legatura MenuItemDish)
        public ICollection<MenuItemDish> MenuItemDishes { get; set; } = new List<MenuItemDish>();

        
    }
}
