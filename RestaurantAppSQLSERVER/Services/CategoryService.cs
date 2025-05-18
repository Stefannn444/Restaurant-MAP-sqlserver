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
    public class CategoryService
    {
        private readonly DbContextFactory _dbContextFactory;

        public CategoryService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        // Metoda pentru a obtine toate categoriile
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Categories.ToListAsync();
            }
        }

        // Metoda pentru a adauga o categorie noua
        public async Task AddCategoryAsync(Category category)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Optional: verifica daca exista deja o categorie cu acelasi nume
                var existingCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);
                if (existingCategory != null)
                {
                    throw new InvalidOperationException($"Categoria cu numele '{category.Name}' exista deja.");
                }

                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }
        }

        // Metoda pentru a actualiza o categorie existenta
        public async Task UpdateCategoryAsync(Category category)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Verifica daca categoria exista in baza de date
                var existingCategory = await context.Categories.FindAsync(category.Id);
                if (existingCategory == null)
                {
                    throw new InvalidOperationException($"Categoria cu ID-ul {category.Id} nu a fost gasita.");
                }

                // Actualizeaza proprietatile (sau foloseste context.Entry(category).State = EntityState.Modified;)
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;

                await context.SaveChangesAsync();
            }
        }

        // Metoda pentru a sterge o categorie
        public async Task DeleteCategoryAsync(int categoryId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var categoryToDelete = await context.Categories.FindAsync(categoryId);
                if (categoryToDelete != null)
                {
                    // Verifica daca exista preparate sau meniuri asociate acestei categorii
                    // Daca ai setat OnDelete(DeleteBehavior.Restrict) in DbContext, baza de date va impiedica stergerea
                    // Poti adauga o verificare explicita aici pentru a oferi un mesaj mai prietenos utilizatorului
                    var hasRelatedDishes = await context.Dishes.AnyAsync(d => d.CategoryId == categoryId);
                    var hasRelatedMenuItems = await context.MenuItems.AnyAsync(mi => mi.CategoryId == categoryId);

                    if (hasRelatedDishes || hasRelatedMenuItems)
                    {
                        throw new InvalidOperationException("Categoria nu poate fi stearsa deoarece are preparate sau meniuri asociate.");
                    }

                    context.Categories.Remove(categoryToDelete);
                    await context.SaveChangesAsync();
                }
            }
        }

        // Aici poti adauga si alte metode utile, cum ar fi GetCategoryByIdAsync etc.
    }
}
