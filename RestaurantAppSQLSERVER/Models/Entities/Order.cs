using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class Order
    {
        public int Id { get; set; } // Id-ul intern al comenzii (PK)

        // Codul unic al comenzii pentru afisare (conform cerintei din barem - nu este Id-ul intern)
        public string OrderCode { get; set; } = string.Empty; // Va trebui generat la plasarea comenzii

        // Cheie straina catre utilizatorul care a plasat comanda
        public int UserId { get; set; }
        // Proprietate de navigare catre utilizator
        public User User { get; set; }

        public string Status { get; set; } = "Inregistrata"; // Starea comenzii (ex: "Inregistrata", "Se pregateste", etc.)

        // Pretul total al comenzii (include item-uri, transport, reduceri).
        // Ar trebui sa fie calculat la plasarea comenzii si actualizat daca starea se schimba (ex: anulare).
        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; } // Data si ora plasarii comenzii
        public DateTime? EstimatedDeliveryTime { get; set; } // Ora estimativa a livrarii (optional)

        // Colectie de item-uri din comanda (Dish-uri si/sau Meniuri)
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Proprietati necesare pentru a pastra istoricul calculului pretului total la momentul comenzii
        public decimal TransportCost { get; set; } // Costul transportului aplicat comenzii
        public decimal DiscountPercentage { get; set; } // Procentul de reducere aplicat comenzii
        // Poti adauga si alte detalii despre reduceri/transport daca este necesar
    }
}
