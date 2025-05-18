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
    // Serviciu pentru gestionarea operatiilor CRUD pe entitatea Dish
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
                // Include Category pentru a putea afisa numele categoriei in UI
                // Include DishAllergens si Allergen pentru a putea gestiona alergenii preparatului
                return await context.Dishes
                                    .Include(d => d.Category)
                                    .Include(d => d.DishAllergens)
                                        .ThenInclude(da => da.Allergen)
                                    .ToListAsync();
            }
        }

        // Metoda pentru a adauga un preparat nou
        public async Task AddDishAsync(Dish dish)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Optional: verifica daca exista deja un preparat cu acelasi nume
                var existingDish = await context.Dishes.FirstOrDefaultAsync(d => d.Name == dish.Name);
                if (existingDish != null)
                {
                    throw new InvalidOperationException($"Preparatul cu numele '{dish.Name}' exista deja.");
                }

                // Ataseaza entitatea Category daca nu este deja urmarita de context
                if (dish.Category != null && context.Entry(dish.Category).State == EntityState.Detached)
                {
                    context.Categories.Attach(dish.Category);
                }

                // Ataseaza entitatile Allergen pentru legaturile DishAllergen daca nu sunt deja urmarite
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

        // Metoda pentru a actualiza un preparat existent
        public async Task UpdateDishAsync(Dish dish)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Verifica daca preparatul exista in baza de date si include relatia cu Alergenii
                var existingDish = await context.Dishes
                                                .Include(d => d.DishAllergens) // Include relatia pentru a o putea actualiza
                                                .FirstOrDefaultAsync(d => d.Id == dish.Id);

                if (existingDish == null)
                {
                    throw new InvalidOperationException($"Preparatul cu ID-ul {dish.Id} nu a fost gasit.");
                }

                // Actualizeaza proprietatile simple
                // Folosim Entry().CurrentValues.SetValues() pentru a actualiza eficient proprietatile simple
                context.Entry(existingDish).CurrentValues.SetValues(dish);

                // Asigura-te ca entitatea Category este urmarita de context daca este necesar
                if (dish.Category != null && context.Entry(dish.Category).State == EntityState.Detached)
                {
                    context.Categories.Attach(dish.Category);
                }
                // Actualizeaza cheia straina CategoryId direct pe existingDish
                existingDish.CategoryId = dish.CategoryId;


                // --- Gestionarea relatiei Many-to-Many cu Alergeni (DishAllergen) ---
                // Compara alergenii existenti cu cei noi si adauga/sterge legaturile din tabela DishAllergen

                // Alergenii existenti in baza de date pentru acest preparat (ID-uri)
                var existingAllergenIds = existingDish.DishAllergens.Select(da => da.AllergenId).ToList();

                // Alergenii noi (din obiectul 'dish' primit, presupunand ca Dish.DishAllergens este populat corect in ViewModel)
                var newAllergenIds = dish.DishAllergens.Select(da => da.AllergenId).ToList();

                // Alergeni de adaugat (care sunt in newAllergenIds dar nu in existingAllergenIds)
                var allergensToAdd = newAllergenIds.Except(existingAllergenIds).ToList();

                // Alergeni de sters (care sunt in existingAllergenIds dar nu in newAllergenIds)
                var allergensToDelete = existingAllergenIds.Except(newAllergenIds).ToList();

                // Adauga noile legaturi in DishAllergen
                foreach (var allergenId in allergensToAdd)
                {
                    // Creeaza o noua legatura DishAllergen
                    existingDish.DishAllergens.Add(new DishAllergen { DishId = existingDish.Id, AllergenId = allergenId });
                }

                // Sterge legaturile care nu mai exista
                foreach (var allergenId in allergensToDelete)
                {
                    var linkToDelete = existingDish.DishAllergens.FirstOrDefault(da => da.AllergenId == allergenId);
                    if (linkToDelete != null)
                    {
                        context.Remove(linkToDelete); // Marcheaza legatura pentru stergere
                    }
                }
                // --- Sfarsit gestionare Alergeni ---

                await context.SaveChangesAsync();
                Debug.WriteLine($"Preparatul cu ID: {dish.Id} actualizat.");
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
                    // Daca ai setat OnDelete(DeleteBehavior.Restrict) pentru relatia Dish -> MenuItemDish,
                    // baza de date va impiedica stergerea daca preparatul este in meniuri.
                    // Poti adauga o verificare explicita aici daca vrei un mesaj mai prietenos.
                    // var isInMenuItems = await context.MenuItemDishes.AnyAsync(mid => mid.DishId == dishId);
                    // if (isInMenuItems) { throw new InvalidOperationException("Preparatul nu poate fi sters deoarece face parte din unul sau mai multe meniuri."); }

                    // Daca ai setat OnDelete(DeleteBehavior.Cascade) pentru relatia Dish -> DishAllergen,
                    // stergerea preparatului va sterge automat legaturile din DishAllergen.

                    context.Dishes.Remove(dishToDelete);
                    await context.SaveChangesAsync();
                    Debug.WriteLine($"Preparatul cu ID: {dishId} sters.");
                }
            }
        }

        // Metoda pentru a obtine un preparat dupa ID (nu mai este strict necesara daca GetAll include tot)
        // Dar o pastram pentru consistenta sau daca e nevoie sa incarcam un singur item cu tot cu relatii
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

        // Aici poti adauga si alte metode utile
    }
}
