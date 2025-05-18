using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RestaurantAppSQLSERVER.Models.Entities
{
    public enum UserRole
    {
        Client = 0,
        Angajat = 1
    }
    public class User
    {
        public int Id { get; set; }
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Email { get; set; }
        public string Nr_tel { get; set; }
        public string Adresa { get; set; }
        public string Parola { get; set; }

        public UserRole Rol { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
