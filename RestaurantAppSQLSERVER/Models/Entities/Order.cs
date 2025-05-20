using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User User { get; set; }

        public string Status { get; set; } = "Inregistrata";

        public decimal Subtotal { get; set; }
        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public decimal TransportCost { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}
