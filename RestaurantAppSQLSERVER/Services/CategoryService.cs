using Microsoft.EntityFrameworkCore;
using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace RestaurantAppSQLSERVER.Services
{
    public class CategoryService
    {
        private readonly DbContextFactory _dbContextFactory;

        public CategoryService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Categories.ToListAsync();
            }
        }
        public async Task AddCategoryAsync(Category category)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    var existingCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);
                    if (existingCategory != null)
                    {
                        throw new InvalidOperationException($"Categoria cu numele '{category.Name}' exista deja.");
                    }
                    var newCategoryIdParam = new SqlParameter("@NewCategoryId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    await context.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC AddCategory @Name={category.Name}, @Description={category.Description}, @NewCategoryId={newCategoryIdParam} OUTPUT"
                    );
                    int newCategoryId = (int)newCategoryIdParam.Value;
                    if (newCategoryId > 0)
                    {
                        category.Id = newCategoryId;
                        System.Diagnostics.Debug.WriteLine($"Categoria '{category.Name}' adaugata cu succes cu ID: {newCategoryId}");
                    }
                    else
                    {
                        throw new InvalidOperationException("Adaugarea categoriei a esuat in procedura stocata.");
                    }

                }
                catch (InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Add Category Error (Duplicate): {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling AddCategory stored procedure: {ex.Message}");
                    throw;
                }
            }
        }
        public async Task UpdateCategoryAsync(Category category)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingCategory = await context.Categories.FindAsync(category.Id);

                if (existingCategory != null)
                {
                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;

                    await context.SaveChangesAsync();
                }
            }
        }
        public async Task DeleteCategoryAsync(int categoryId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var categoryToDelete = await context.Categories.FindAsync(categoryId);
                if (categoryToDelete != null)
                {
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
    }
}
