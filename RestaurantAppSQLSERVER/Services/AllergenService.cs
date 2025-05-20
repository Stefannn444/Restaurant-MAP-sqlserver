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
        public async Task<List<Allergen>> GetAllAllergensAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Allergens.ToListAsync();
            }
        }
        public async Task AddAllergenAsync(Allergen allergen)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingAllergen = await context.Allergens.FirstOrDefaultAsync(a => a.Name == allergen.Name);
                if (existingAllergen != null)
                {
                    throw new InvalidOperationException($"Alergenul cu numele '{allergen.Name}' exista deja.");
                }

                context.Allergens.Add(allergen);
                await context.SaveChangesAsync();
            }
        }
        public async Task UpdateAllergenAsync(Allergen allergen)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingAllergen = await context.Allergens.FindAsync(allergen.Id);
                if (existingAllergen == null)
                {
                    throw new InvalidOperationException($"Alergenul cu ID-ul {allergen.Id} nu a fost gasit.");
                }
                existingAllergen.Name = allergen.Name;

                await context.SaveChangesAsync();
            }
        }
        public async Task DeleteAllergenAsync(int allergenId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var allergenToDelete = await context.Allergens.FindAsync(allergenId);
                if (allergenToDelete != null)
                {

                    context.Allergens.Remove(allergenToDelete);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
