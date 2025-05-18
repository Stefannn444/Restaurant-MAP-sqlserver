using Microsoft.EntityFrameworkCore;
using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Services
{
    public class DishService
    {
        private readonly DbContextFactory _dbContextFactory;

        public DishService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        // Metoda pentru a obtine toate preparatele
        public async Task<List<Dish>> GetAllDishesAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Include Category pentru a putea afisa numele categoriei daca este necesar
                return await context.Dishes
                                    .Include(d => d.Category)
                                    .ToListAsync();
            }
        }

        // Metoda pentru a adauga un preparat nou
        public async Task AddDishAsync(Dish dish)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Dishes.Add(dish);
                await context.SaveChangesAsync();
            }
        }

        // Metoda pentru a actualiza un preparat existent
        public async Task UpdateDishAsync(Dish dish)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Dishes.Update(dish);
                await context.SaveChangesAsync();
            }
        }

        // Metoda pentru a sterge un preparat
        public async Task DeleteDishAsync(int dishId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var dishToDelete = await context.Dishes.FindAsync(dishId);
                if (dishToDelete != null)
                {
                    context.Dishes.Remove(dishToDelete);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
