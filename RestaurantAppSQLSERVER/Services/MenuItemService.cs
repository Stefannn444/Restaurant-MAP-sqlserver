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
    // Serviciu pentru gestionarea operatiilor CRUD pe entitatea MenuItem (care este Meniul principal)
    public class MenuItemService
    {
        private readonly DbContextFactory _dbContextFactory;

        public MenuItemService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        // Metoda pentru a obtine toate Meniurile (MenuItem)
        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Include relatia cu MenuItemDish si Preparatele asociate (Dish)
                return await context.MenuItems // Selectam din tabela MenuItem (Meniuri)
                                    .Include(mi => mi.Category) // Include Categoria (daca o afisezi in DataGrid)
                                    .Include(mi => mi.MenuItemDishes) // Include legaturile MenuItemDish
                                        .ThenInclude(mid => mid.Dish) // Include Dish-ul pentru fiecare legatura MenuItemDish
                                    .ToListAsync();
            }
        }

        // Metoda pentru a adauga un Meniu (MenuItem) nou
        public async Task AddMenuItemAsync(MenuItem menuItem)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Optional: verifica daca exista deja un meniu cu acelasi nume
                var existingMenuItem = await context.MenuItems.FirstOrDefaultAsync(m => m.Name == menuItem.Name);
                if (existingMenuItem != null)
                {
                    throw new InvalidOperationException($"Meniul cu numele '{menuItem.Name}' exista deja.");
                }

                // Ataseaza entitatile legate (Category, MenuItemDishes, Dish) daca nu sunt deja urmarite
                if (menuItem.Category != null && context.Entry(menuItem.Category).State == EntityState.Detached)
                {
                    context.Categories.Attach(menuItem.Category);
                }

                if (menuItem.MenuItemDishes != null)
                {
                    foreach (var menuItemDish in menuItem.MenuItemDishes)
                    {
                        // Ataseaza Dish-ul pentru fiecare legatura MenuItemDish
                        if (menuItemDish.Dish != null && context.Entry(menuItemDish.Dish).State == EntityState.Detached)
                        {
                            context.Dishes.Attach(menuItemDish.Dish);
                        }
                        // MenuItemDish-urile noi vor fi adaugate automat cand adaugi MenuItem
                        // Cantitatea (Quantity) ar trebui sa fie deja setata corect in ViewModel la crearea menuItemDish
                    }
                }

                context.MenuItems.Add(menuItem); // Adaugam noul MenuItem (Meniu)
                await context.SaveChangesAsync();
                Debug.WriteLine($"Meniul '{menuItem.Name}' adaugat cu ID: {menuItem.Id}");
            }
        }

        // Metoda pentru a actualiza un Meniu (MenuItem) existent
        public async Task UpdateMenuItemAsync(MenuItem menuItem)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Verifica daca meniul exista in baza de date si include relatia cu MenuItemDish-urile
                var existingMenuItem = await context.MenuItems
                                                .Include(mi => mi.MenuItemDishes) // Include relatia pentru a o putea actualiza
                                                .FirstOrDefaultAsync(m => m.Id == menuItem.Id);

                if (existingMenuItem == null)
                {
                    throw new InvalidOperationException($"Meniul cu ID-ul {menuItem.Id} nu a fost gasit.");
                }

                // Actualizeaza proprietatile simple ale Meniului (MenuItem)
                // Excludem Description pentru ca nu exista in entitate
                context.Entry(existingMenuItem).CurrentValues.SetValues(menuItem);

                // Asigura-te ca entitatea Category este urmarita de context daca este necesar
                if (menuItem.Category != null && context.Entry(menuItem.Category).State == EntityState.Detached)
                {
                    context.Categories.Attach(menuItem.Category);
                }
                // Actualizeaza cheia straina CategoryId direct pe existingMenuItem
                existingMenuItem.CategoryId = menuItem.CategoryId;


                // --- Gestionarea relatiei Many-to-Many cu Preparate (prin MenuItemDish) ---
                // Compara MenuItemDish-urile existente cu cele noi si adauga/sterge/actualizeaza legaturile din tabela MenuItemDish

                // Creeaza dictionare pentru acces rapid
                var existingMenuItemDishesDict = existingMenuItem.MenuItemDishes.ToDictionary(mid => mid.DishId);
                var newMenuItemDishesDict = menuItem.MenuItemDishes?.ToDictionary(mid => mid.DishId) ?? new Dictionary<int, MenuItemDish>();


                // Lista de DishId-uri din meniul actualizat
                var newDishIds = newMenuItemDishesDict.Keys.ToList();

                // Lista de DishId-uri din meniul existent in baza de date
                var existingDishIds = existingMenuItemDishesDict.Keys.ToList();

                // DishId-uri de adaugat (care sunt in newDishIds dar nu in existingDishIds)
                var dishIdsToAdd = newDishIds.Except(existingDishIds).ToList();

                // DishId-uri de sters (care sunt in existingDishIds dar nu in newDishIds)
                var dishIdsToDelete = existingDishIds.Except(newDishIds).ToList();

                // DishId-uri de actualizat (care sunt in ambele liste)
                var dishIdsToUpdate = existingDishIds.Intersect(newDishIds).ToList();


                // Adauga noile legaturi in MenuItemDish
                foreach (var dishId in dishIdsToAdd)
                {
                    // Creeaza un nou MenuItemDish folosind datele din meniul primit
                    // Quantity ar trebui sa fie setat corect in ViewModel la crearea menuItemDish
                    if (newMenuItemDishesDict.TryGetValue(dishId, out var newMenuItemDish))
                    {
                        existingMenuItem.MenuItemDishes.Add(new MenuItemDish
                        {
                            MenuItemId = existingMenuItem.Id,
                            DishId = dishId,
                            Quantity = newMenuItemDish.Quantity // Seteaza cantitatea din obiectul nou
                        });
                    }
                }

                // Sterge legaturile care nu mai exista
                foreach (var dishId in dishIdsToDelete)
                {
                    if (existingMenuItemDishesDict.TryGetValue(dishId, out var itemToDelete))
                    {
                        context.Remove(itemToDelete); // Marcheaza MenuItemDish pentru stergere
                    }
                }

                // NOU: Actualizeaza Cantitatea pentru legaturile MenuItemDish care exista deja
                foreach (var dishId in dishIdsToUpdate)
                {
                    // Gaseste legatura existenta si legatura noua
                    if (existingMenuItemDishesDict.TryGetValue(dishId, out var existingMenuItemDish) &&
                        newMenuItemDishesDict.TryGetValue(dishId, out var newMenuItemDish))
                    {
                        // Actualizeaza cantitatea pe obiectul existent din context
                        existingMenuItemDish.Quantity = newMenuItemDish.Quantity;
                        // EF Core va detecta schimbarea si o va salva
                    }
                }


                // --- Sfarsit gestionare MenuItemDish ---

                await context.SaveChangesAsync();
                Debug.WriteLine($"Meniul cu ID: {menuItem.Id} actualizat.");
            }
        }

        // Metoda pentru a sterge un Meniu (MenuItem)
        public async Task DeleteMenuItemAsync(int menuItemId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var menuItemToDelete = await context.MenuItems.FindAsync(menuItemId); // Cautam in tabela MenuItem
                if (menuItemToDelete != null)
                {
                    // Daca ai setat OnDelete(DeleteBehavior.Cascade) pentru relatia MenuItem -> MenuItemDish,
                    // stergerea Meniului (MenuItem) va sterge automat MenuItemDish-urile asociate.

                    context.MenuItems.Remove(menuItemToDelete);
                    await context.SaveChangesAsync();
                    Debug.WriteLine($"Meniul cu ID: {menuItemId} sters.");
                }
            }
        }

        // Metoda pentru a obtine un Meniu (MenuItem) dupa ID (nu mai este strict necesara daca GetAll include tot)
        public async Task<MenuItem> GetMenuItemByIdAsync(int menuItemId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.MenuItems
                                    .Include(mi => mi.Category)
                                    .Include(mi => mi.MenuItemDishes)
                                        .ThenInclude(mid => mid.Dish) // Include Dish-ul pentru fiecare MenuItemDish
                                    .FirstOrDefaultAsync(m => m.Id == menuItemId);
            }
        }

        // Aici poti adauga si alte metode utile
    }
}
