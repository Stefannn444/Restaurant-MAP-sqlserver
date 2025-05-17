using Microsoft.EntityFrameworkCore;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Data
{
    public class RestaurantDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Exemplu: seeding pentru testare
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Nume = "Ion",
                    Prenume = "Popescu",
                    Email = "ion.popescu@example.com",
                    Nr_tel = "0712345678",
                    Adresa = "Strada Exemplu 1",
                    Parola = "parola123", // Parola reală ar trebui să fie hash-uită!
                    Rol = UserRole.Client
                }
            );
        }
    }
}
