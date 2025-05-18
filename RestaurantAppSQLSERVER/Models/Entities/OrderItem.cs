using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class OrderItem
    {
        public int Id { get; set; } // Id-ul unic al item-ului in cadrul comenzii (PK)

        // Cheie straina catre comanda din care face parte
        public int OrderId { get; set; }
        // Proprietate de navigare catre comanda
        public Order Order { get; set; }

        public string ItemType { get; set; } = string.Empty; // "Dish" sau "MenuItem" - pentru a sti la ce entitate se refera ItemId
        public int ItemId { get; set; } // Id-ul Dish-ului sau MenuItem-ului comandat

        public string Name { get; set; } = string.Empty; // Numele item-ului la momentul comenzii (pentru a pastra istoricul)
        public decimal Price { get; set; } // Pretul unitar al item-ului la momentul comenzii (pentru a pastra istoricul)
        public int Quantity { get; set; } // Numarul de bucati comandate din acest item

        // TotalPrice pentru acest item este Price * Quantity.
        // Il stocam aici pentru performanta, dar ar trebui sa fie calculat si actualizat la plasarea comenzii.
        public decimal TotalPrice { get; set; } // Total Price pentru acest item (Price * Quantity)
    }
}
