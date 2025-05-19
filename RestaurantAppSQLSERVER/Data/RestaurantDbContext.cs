using Microsoft.EntityFrameworkCore;
using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.Models.Wrappers; // Adaugat pentru DisplayMenuItem
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

        // NU adaugi DbSet<DisplayMenuItem> aici, deoarece nu este o entitate de baza de date

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
                .HasKey(da => new { da.DishId, da.AllergenId }); // Cheie compusa

            modelBuilder.Entity<DishAllergen>()
                .HasOne(da => da.Dish)
                .WithMany(d => d.DishAllergens)
                .HasForeignKey(da => da.DishId)
                .OnDelete(DeleteBehavior.Restrict); // Sau Cascade, in functie de logica dorita

            modelBuilder.Entity<DishAllergen>()
                .HasOne(da => da.Allergen)
                .WithMany(a => a.DishAllergens)
                .HasForeignKey(da => da.AllergenId)
                .OnDelete(DeleteBehavior.Restrict); // Sau Cascade

            // Relatia Many-to-Many intre MenuItem si Dish (prin MenuItemDish)
            modelBuilder.Entity<MenuItemDish>()
                .HasKey(mid => new { mid.MenuItemId, mid.DishId }); // Cheie compusa

            modelBuilder.Entity<MenuItemDish>()
                .HasOne(mid => mid.MenuItem)
                .WithMany(mi => mi.MenuItemDishes)
                .HasForeignKey(mid => mid.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade); // Stergerea unui MenuItem sterge legaturile

            modelBuilder.Entity<MenuItemDish>()
                .HasOne(mid => mid.Dish)
                .WithMany(d => d.MenuItemDishes)
                .HasForeignKey(mid => mid.DishId)
                .OnDelete(DeleteBehavior.Restrict); // Nu sterge Dish-ul daca face parte dintr-un MenuItem

            // Relatia One-to-Many intre Category si Dish
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Dishes)
                .WithOne(d => d.Category)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Nu sterge Categoria daca are Dish-uri

            // Relatia One-to-Many intre Category si MenuItem
            modelBuilder.Entity<Category>()
               .HasMany(c => c.MenuItems)
               .WithOne(mi => mi.Category)
               .HasForeignKey(mi => mi.CategoryId)
               .OnDelete(DeleteBehavior.Restrict); // Nu sterge Categoria daca are Meniuri

            // Relatia One-to-Many intre User si Order
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Nu sterge User-ul daca are Comenzi

            // Relatia One-to-Many intre Order si OrderItem
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Stergerea unei Comenzi sterge OrderItem-urile

            // --- Configurarea Tipului Fara Cheie (Keyless Entity Type) pentru DisplayMenuItem ---
            // Acest lucru permite maparea rezultatelor unui query/SP la clasa DisplayMenuItem
            // chiar daca aceasta nu corespunde unei tabele si nu are cheie primara.
            modelBuilder.Entity<DisplayMenuItem>().HasNoKey();

            // --- Seeding Initial (Optional) ---
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Nume = "Client",
                    Prenume = "Exemplu",
                    Email = "client@exemplu.com",
                    Nr_tel = "0700000000",
                    Adresa = "Strada Exemplu 1",
                    Parola = "parola123", // Parola reală ar trebui să fie hash-uită!
                    Rol = UserRole.Client
                }
                 // Adauga si un angajat pentru testare
                 , new User
                 {
                     Id = 2,
                     Nume = "Angajat",
                     Prenume = "Restaurant",
                     Email = "angajat@exemplu.com",
                     Nr_tel = "0711111111",
                     Adresa = "Sediul Restaurantului",
                     Parola = "parolaangajat", // Parola reală ar trebui să fie hash-uită!
                     Rol = UserRole.Angajat
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
            // modelBuilder.Entity<DishAllergen>().HasData( new DishAllergen { DishId = 1, AllergenId = ... } );
            // modelBuilder.Entity<MenuItemDish>().HasData( new MenuItemDish { MenuItemId = ..., DishId = ..., Quantity = ... } );


        }
    }
}
