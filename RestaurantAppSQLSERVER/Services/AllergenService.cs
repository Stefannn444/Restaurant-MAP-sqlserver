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
    public class AllergenService
    {
        private readonly DbContextFactory _dbContextFactory;

        public AllergenService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        // Metoda pentru a obtine toti alergenii
        public async Task<List<Allergen>> GetAllAllergensAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Allergens.ToListAsync();
            }
        }

        // Metoda pentru a adauga un alergen nou
        public async Task AddAllergenAsync(Allergen allergen)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Optional: verifica daca exista deja un alergen cu acelasi nume
                var existingAllergen = await context.Allergens.FirstOrDefaultAsync(a => a.Name == allergen.Name);
                if (existingAllergen != null)
                {
                    throw new InvalidOperationException($"Alergenul cu numele '{allergen.Name}' exista deja.");
                }

                context.Allergens.Add(allergen);
                await context.SaveChangesAsync();
            }
        }

        // Metoda pentru a actualiza un alergen existent
        public async Task UpdateAllergenAsync(Allergen allergen)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Verifica daca alergenul exista in baza de date
                var existingAllergen = await context.Allergens.FindAsync(allergen.Id);
                if (existingAllergen == null)
                {
                    throw new InvalidOperationException($"Alergenul cu ID-ul {allergen.Id} nu a fost gasit.");
                }

                // Actualizeaza proprietatile
                existingAllergen.Name = allergen.Name;

                await context.SaveChangesAsync();
            }
        }

        // Metoda pentru a sterge un alergen
        public async Task DeleteAllergenAsync(int allergenId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var allergenToDelete = await context.Allergens.FindAsync(allergenId);
                if (allergenToDelete != null)
                {
                    // Verifica daca exista preparate asociate acestui alergen (prin tabela de legatura DishAllergen)
                    // Daca ai setat OnDelete(DeleteBehavior.Cascade) pe relatia DishAllergen -> Allergen,
                    // stergerea alergenului va sterge automat legaturile din DishAllergen.
                    // Poti adauga o verificare explicita aici daca vrei sa previi stergerea daca sunt legaturi active.
                    // var hasRelatedDishes = await context.DishAllergens.AnyAsync(da => da.AllergenId == allergenId);
                    // if (hasRelatedDishes)
                    // {
                    //     throw new InvalidOperationException("Alergenul nu poate fi sters deoarece este asociat unor preparate.");
                    // }

                    context.Allergens.Remove(allergenToDelete);
                    await context.SaveChangesAsync();
                }
            }
        }

        // Aici poti adauga si alte metode utile
    }
}
