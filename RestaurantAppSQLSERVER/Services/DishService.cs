using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace RestaurantAppSQLSERVER.Services
{
    public class DishService
    {
        private readonly DbContextFactory _dbContextFactory;

        public DishService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }
        public async Task<List<Dish>> GetAllDishesAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Dishes
                                    .Include(d => d.Category)
                                    .Include(d => d.DishAllergens)
                                        .ThenInclude(da => da.Allergen)
                                    .ToListAsync();
            }
        }
        public async Task AddDishAsync(Dish dish)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingDish = await context.Dishes.FirstOrDefaultAsync(d => d.Name == dish.Name);
                if (existingDish != null)
                {
                    throw new InvalidOperationException($"Preparatul cu numele '{dish.Name}' exista deja.");
                }
                if (dish.Category != null && context.Entry(dish.Category).State == EntityState.Detached)
                {
                    context.Categories.Attach(dish.Category);
                }
                if (dish.DishAllergens != null)
                {
                    foreach (var dishAllergen in dish.DishAllergens)
                    {
                        if (dishAllergen.Allergen != null && context.Entry(dishAllergen.Allergen).State == EntityState.Detached)
                        {
                            context.Allergens.Attach(dishAllergen.Allergen);
                        }
                    }
                }


                context.Dishes.Add(dish);
                await context.SaveChangesAsync();
                Debug.WriteLine($"Preparatul '{dish.Name}' adaugat cu ID: {dish.Id}");
            }
        }
        public async Task UpdateDishAsync(Dish dish)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingDish = await context.Dishes
                                                .Include(d => d.DishAllergens)
                                                .FirstOrDefaultAsync(d => d.Id == dish.Id);

                if (existingDish == null)
                {
                    throw new InvalidOperationException($"Preparatul cu ID-ul {dish.Id} nu a fost gasit.");
                }
                context.Entry(existingDish).CurrentValues.SetValues(dish);
                if (dish.Category != null && context.Entry(dish.Category).State == EntityState.Detached)
                {
                    context.Categories.Attach(dish.Category);
                }
                existingDish.CategoryId = dish.CategoryId;
                var existingAllergenIds = existingDish.DishAllergens.Select(da => da.AllergenId).ToList();
                var newAllergenIds = dish.DishAllergens.Select(da => da.AllergenId).ToList();
                var allergensToAdd = newAllergenIds.Except(existingAllergenIds).ToList();
                var allergensToDelete = existingAllergenIds.Except(newAllergenIds).ToList();
                foreach (var allergenId in allergensToAdd)
                {
                    existingDish.DishAllergens.Add(new DishAllergen { DishId = existingDish.Id, AllergenId = allergenId });
                }
                foreach (var allergenId in allergensToDelete)
                {
                    var linkToDelete = existingDish.DishAllergens.FirstOrDefault(da => da.AllergenId == allergenId);
                    if (linkToDelete != null)
                    {
                        context.Remove(linkToDelete);
                    }
                }

                await context.SaveChangesAsync();
                Debug.WriteLine($"Preparatul cu ID: {dish.Id} actualizat.");
            }
        }
        public async Task DeleteDishAsync(int dishId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var dishToDelete = await context.Dishes.FindAsync(dishId);
                if (dishToDelete != null)
                {

                    context.Dishes.Remove(dishToDelete);
                    await context.SaveChangesAsync();
                    Debug.WriteLine($"Preparatul cu ID: {dishId} sters.");
                }
            }
        }
        public async Task<Dish> GetDishByIdAsync(int dishId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Dishes
                                    .Include(d => d.Category)
                                    .Include(d => d.DishAllergens)
                                        .ThenInclude(da => da.Allergen)
                                    .FirstOrDefaultAsync(d => d.Id == dishId);
            }
        }
    }
}
