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
    public class MenuItemService
    {
        private readonly DbContextFactory _dbContextFactory;

        public MenuItemService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }
        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.MenuItems
                                    .Include(mi => mi.Category)
                                    .Include(mi => mi.MenuItemDishes)
                                        .ThenInclude(mid => mid.Dish)
                                    .ToListAsync();
            }
        }
        public async Task AddMenuItemAsync(MenuItem menuItem)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingMenuItem = await context.MenuItems.FirstOrDefaultAsync(m => m.Name == menuItem.Name);
                if (existingMenuItem != null)
                {
                    throw new InvalidOperationException($"Meniul cu numele '{menuItem.Name}' exista deja.");
                }
                if (menuItem.Category != null && context.Entry(menuItem.Category).State == EntityState.Detached)
                {
                    context.Categories.Attach(menuItem.Category);
                }

                if (menuItem.MenuItemDishes != null)
                {
                    foreach (var menuItemDish in menuItem.MenuItemDishes)
                    {
                        if (menuItemDish.Dish != null && context.Entry(menuItemDish.Dish).State == EntityState.Detached)
                        {
                            context.Dishes.Attach(menuItemDish.Dish);
                        }
                    }
                }

                context.MenuItems.Add(menuItem);
                await context.SaveChangesAsync();
                Debug.WriteLine($"Meniul '{menuItem.Name}' adaugat cu ID: {menuItem.Id}");
            }
        }
        public async Task UpdateMenuItemAsync(MenuItem menuItem)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingMenuItem = await context.MenuItems
                                                .Include(mi => mi.MenuItemDishes)
                                                .FirstOrDefaultAsync(m => m.Id == menuItem.Id);

                if (existingMenuItem == null)
                {
                    throw new InvalidOperationException($"Meniul cu ID-ul {menuItem.Id} nu a fost gasit.");
                }
                context.Entry(existingMenuItem).CurrentValues.SetValues(menuItem);
                if (menuItem.Category != null && context.Entry(menuItem.Category).State == EntityState.Detached)
                {
                    context.Categories.Attach(menuItem.Category);
                }
                existingMenuItem.CategoryId = menuItem.CategoryId;
                var existingMenuItemDishesDict = existingMenuItem.MenuItemDishes.ToDictionary(mid => mid.DishId);
                var newMenuItemDishesDict = menuItem.MenuItemDishes?.ToDictionary(mid => mid.DishId) ?? new Dictionary<int, MenuItemDish>();
                var newDishIds = newMenuItemDishesDict.Keys.ToList();
                var existingDishIds = existingMenuItemDishesDict.Keys.ToList();
                var dishIdsToAdd = newDishIds.Except(existingDishIds).ToList();
                var dishIdsToDelete = existingDishIds.Except(newDishIds).ToList();
                var dishIdsToUpdate = existingDishIds.Intersect(newDishIds).ToList();
                foreach (var dishId in dishIdsToAdd)
                {
                    if (newMenuItemDishesDict.TryGetValue(dishId, out var newMenuItemDish))
                    {
                        existingMenuItem.MenuItemDishes.Add(new MenuItemDish
                        {
                            MenuItemId = existingMenuItem.Id,
                            DishId = dishId,
                            Quantity = newMenuItemDish.Quantity
                        });
                    }
                }
                foreach (var dishId in dishIdsToDelete)
                {
                    if (existingMenuItemDishesDict.TryGetValue(dishId, out var itemToDelete))
                    {
                        context.Remove(itemToDelete);
                    }
                }
                foreach (var dishId in dishIdsToUpdate)
                {
                    if (existingMenuItemDishesDict.TryGetValue(dishId, out var existingMenuItemDish) &&
                        newMenuItemDishesDict.TryGetValue(dishId, out var newMenuItemDish))
                    {
                        existingMenuItemDish.Quantity = newMenuItemDish.Quantity;
                    }
                }

                await context.SaveChangesAsync();
                Debug.WriteLine($"Meniul cu ID: {menuItem.Id} actualizat.");
            }
        }
        public async Task DeleteMenuItemAsync(int menuItemId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var menuItemToDelete = await context.MenuItems.FindAsync(menuItemId);
                if (menuItemToDelete != null)
                {

                    context.MenuItems.Remove(menuItemToDelete);
                    await context.SaveChangesAsync();
                    Debug.WriteLine($"Meniul cu ID: {menuItemId} sters.");
                }
            }
        }
        public async Task<MenuItem> GetMenuItemByIdAsync(int menuItemId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.MenuItems
                                    .Include(mi => mi.Category)
                                    .Include(mi => mi.MenuItemDishes)
                                        .ThenInclude(mid => mid.Dish)
                                    .FirstOrDefaultAsync(m => m.Id == menuItemId);
            }
        }
    }
}
