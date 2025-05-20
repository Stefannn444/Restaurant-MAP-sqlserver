using Microsoft.EntityFrameworkCore;
using RestaurantAppSQLSERVER.Models.Entities;
using RestaurantAppSQLSERVER.Models.Wrappers;
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
        public DbSet<Allergen> Allergens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishAllergen> DishAllergens { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemDish> MenuItemDishes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DishAllergen>()
                .HasKey(da => new { da.DishId, da.AllergenId });

            modelBuilder.Entity<DishAllergen>()
                .HasOne(da => da.Dish)
                .WithMany(d => d.DishAllergens)
                .HasForeignKey(da => da.DishId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DishAllergen>()
                .HasOne(da => da.Allergen)
                .WithMany(a => a.DishAllergens)
                .HasForeignKey(da => da.AllergenId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<MenuItemDish>()
                .HasKey(mid => new { mid.MenuItemId, mid.DishId });

            modelBuilder.Entity<MenuItemDish>()
                .HasOne(mid => mid.MenuItem)
                .WithMany(mi => mi.MenuItemDishes)
                .HasForeignKey(mid => mid.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MenuItemDish>()
                .HasOne(mid => mid.Dish)
                .WithMany(d => d.MenuItemDishes)
                .HasForeignKey(mid => mid.DishId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Dishes)
                .WithOne(d => d.Category)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Category>()
               .HasMany(c => c.MenuItems)
               .WithOne(mi => mi.Category)
               .HasForeignKey(mi => mi.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DisplayMenuItem>().HasNoKey();
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Nume = "Client",
                    Prenume = "Exemplu",
                    Email = "client@exemplu.com",
                    Nr_tel = "0700000000",
                    Adresa = "Strada Exemplu 1",
                    Parola = "parola123",
                    Rol = UserRole.Client
                }
                 , new User
                 {
                     Id = 2,
                     Nume = "Angajat",
                     Prenume = "Restaurant",
                     Email = "angajat@exemplu.com",
                     Nr_tel = "0711111111",
                     Adresa = "Sediul Restaurantului",
                     Parola = "parolaangajat",
                     Rol = UserRole.Angajat
                 }
            );


        }
    }
}
