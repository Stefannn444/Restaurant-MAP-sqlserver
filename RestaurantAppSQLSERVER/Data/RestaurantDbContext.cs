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
        // DbSet pentru fiecare entitate din modelul tau
        public DbSet<User> Users { get; set; }
        public DbSet<Allergen> Allergens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishAllergen> DishAllergens { get; set; } // DbSet pentru tabela de legatura Dish-Allergen
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemDish> MenuItemDishes { get; set; } // DbSet pentru tabela de legatura MenuItem-Dish
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configurarea Relatiilor ---

            // Relatia Many-to-Many intre Dish si Allergen (prin DishAllergen)
            modelBuilder.Entity<DishAllergen>()
                .HasKey(da => new { da.DishId, da.AllergenId }); // Defineste cheia primara compusa

            modelBuilder.Entity<DishAllergen>()
                .HasOne(da => da.Dish) // O legatura DishAllergen are un singur Dish
                .WithMany(d => d.DishAllergens) // Un Dish poate avea multe legaturi DishAllergen
                .HasForeignKey(da => da.DishId)
                .OnDelete(DeleteBehavior.Cascade); // Cand stergi un Dish, sterge legaturile din DishAllergen

            modelBuilder.Entity<DishAllergen>()
                .HasOne(da => da.Allergen) // O legatura DishAllergen are un singur Allergen
                .WithMany(a => a.DishAllergens) // Un Allergen poate avea multe legaturi DishAllergen
                .HasForeignKey(da => da.AllergenId)
                .OnDelete(DeleteBehavior.Cascade); // Cand stergi un Allergen, sterge legaturile din DishAllergen

            // Relatia Many-to-Many intre MenuItem si Dish (prin MenuItemDish)
            modelBuilder.Entity<MenuItemDish>()
                .HasKey(mid => new { mid.MenuItemId, mid.DishId }); // Defineste cheia primara compusa

            modelBuilder.Entity<MenuItemDish>()
                .HasOne(mid => mid.MenuItem) // O legatura MenuItemDish are un singur MenuItem
                .WithMany(mi => mi.MenuItemDishes) // Un MenuItem poate avea multe legaturi MenuItemDish
                .HasForeignKey(mid => mid.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade); // Cand stergi un MenuItem, sterge legaturile din MenuItemDish

            modelBuilder.Entity<MenuItemDish>()
                .HasOne(mid => mid.Dish) // O legatura MenuItemDish are un singur Dish
                .WithMany(d => d.MenuItemDishes) // Un Dish poate fi in multe legaturi MenuItemDish (in meniuri diferite)
                .HasForeignKey(mid => mid.DishId)
                .OnDelete(DeleteBehavior.Restrict); // <--- FIX: Seteaza pe Restrict pentru a evita ciclul

            // Relatia One-to-Many intre Category si Dish
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Dishes) // O Categorie are multe Dish-uri
                .WithOne(d => d.Category) // Un Dish are o singura Categorie
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Seteaza pe Restrict pentru a evita stergerea in cascada a Dish-urilor la stergerea Categoriei

            // Relatia One-to-Many intre Category si MenuItem
            modelBuilder.Entity<Category>()
               .HasMany(c => c.MenuItems) // O Categorie are multe MenuItem-uri
               .WithOne(mi => mi.Category) // Un MenuItem are o singura Categorie
               .HasForeignKey(mi => mi.CategoryId)
               .OnDelete(DeleteBehavior.Restrict); // Seteaza pe Restrict pentru a evita stergerea in cascada a MenuItem-urilor la stergerea Categoriei

            // Relatia One-to-Many intre User si Order
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders) // Un User are multe Orders
                .WithOne(o => o.User) // Un Order are un singur User
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cand stergi un User, sterge comenzile sale

            // Relatia One-to-Many intre Order si OrderItem
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems) // Un Order are multe OrderItems
                .WithOne(oi => oi.Order) // Un OrderItem are un singur Order
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Cand stergi un Order, sterge item-urile din el

            // --- Configurari suplimentare (optional, dar recomandat) ---

            // Asigură-te că OrderCode este unic (dacă vrei să fie un identificator unic)
            modelBuilder.Entity<Order>()
                 .HasIndex(o => o.OrderCode)
                 .IsUnique(); // Adaugat index unic pentru OrderCode

            // Configurează precizia pentru tipurile decimal (pentru preturi)
            modelBuilder.Entity<Dish>()
                .Property(d => d.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MenuItem>()
                .Property(mi => mi.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
               .Property(o => o.TotalPrice)
               .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
               .Property(o => o.TransportCost)
               .HasPrecision(18, 2);

            // --- Seeding pentru testare (pastrat) ---
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

            // Adaugă seeding pentru alte entitati daca vrei sa ai date initiale in baza de date (optional)
            // Exemplu:
            // modelBuilder.Entity<Category>().HasData(
            //     new Category { Id = 1, Name = "Supa", Description = "Diverse supe si ciorbe" },
            //     new Category { Id = 2, Name = "Desert", Description = "Dulciuri delicioase" }
            // );

            // modelBuilder.Entity<Dish>().HasData(
            //     new Dish
            //     {
            //         Id = 1,
            //         Name = "Supa crema de ciuperci",
            //         Price = 15.50m,
            //         Quantity = 300, // grame
            //         TotalQuantity = 3000, // grame in total in restaurant
            //         CategoryId = 1, // Categoria "Supa"
            //         PhotoPath = null,
            //         IsAvailable = true,
            //         Description = "Supa cremoasa de ciuperci proaspete"
            //     }
            // );

            // Seeding pentru tabelele de legatura (daca adaugi date pentru Dish si Allergen/MenuItem)
            // modelBuilder.Entity<DishAllergen>().HasData(...)
            // modelBuilder.Entity<MenuItemDish>().HasData(...)
        }
    }
}
