using Microsoft.EntityFrameworkCore;
using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient; // NECESARA pentru SqlParameter
using System.Data;
using System.Diagnostics; // NECESARA pentru ParameterDirection

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

        // Metoda pentru a adauga o categorie noua folosind Procedura Stocata AddCategory cu parametru OUTPUT
        public async Task AddCategoryAsync(Category category)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    // Optional: verifica daca exista deja o categorie cu acelasi nume inainte de a apela SP-ul
                    // Aceasta verificare poate fi mutata si in SP daca vrei ca SP-ul sa gestioneze unicitatea
                    var existingCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);
                    if (existingCategory != null)
                    {
                        throw new InvalidOperationException($"Categoria cu numele '{category.Name}' exista deja.");
                    }

                    // Declara parametrul de output pentru ID-ul nou creat
                    // Trebuie sa specifici tipul SQL si directia (Output)
                    var newCategoryIdParam = new SqlParameter("@NewCategoryId", SqlDbType.Int) { Direction = ParameterDirection.Output };

                    // Apeleaza procedura stocata folosind ExecuteSqlInterpolatedAsync
                    // Pasam parametrii de intrare si parametrul de output
                    // Adaugam "OUTPUT" la finalul parametrului in string-ul interpolat
                    await context.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC AddCategory @Name={category.Name}, @Description={category.Description}, @NewCategoryId={newCategoryIdParam} OUTPUT"
                    );

                    // Dupa executie, valoarea parametrului de output este disponibila in newCategoryIdParam.Value
                    // Convertim valoarea la int
                    int newCategoryId = (int)newCategoryIdParam.Value;

                    // Daca procedura a returnat un ID valid (mai mare ca 0), inseamna succes
                    if (newCategoryId > 0)
                    {
                        // Optional: seteaza ID-ul pe obiectul category primit
                        category.Id = newCategoryId; // Seteaza ID-ul pe obiectul original
                        System.Diagnostics.Debug.WriteLine($"Categoria '{category.Name}' adaugata cu succes cu ID: {newCategoryId}");
                    }
                    else
                    {
                        // Ceva a mers prost in SP sau nu a returnat un ID valid (desi SCOPE_IDENTITY ar trebui sa functioneze)
                        throw new InvalidOperationException("Adaugarea categoriei a esuat in procedura stocata.");
                    }

                }
                catch (InvalidOperationException ex)
                {
                    // Prinde exceptia aruncata de verificarea unicitatii (daca o faci in C#)
                    System.Diagnostics.Debug.WriteLine($"Add Category Error (Duplicate): {ex.Message}");
                    throw; // Arunca exceptia mai departe
                }
                catch (Exception ex)
                {
                    // Gestioneaza alte erori care pot aparea la apelarea procedurii stocate
                    System.Diagnostics.Debug.WriteLine($"Error calling AddCategory stored procedure: {ex.Message}");
                    throw; // Arunca exceptia mai departe
                }
            }
        }

        // Metoda pentru a actualiza o categorie existenta
        public async Task UpdateCategoryAsync(Category category)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Caută categoria existentă
                var existingCategory = await context.Categories.FindAsync(category.Id);

                if (existingCategory != null)
                {
                    // Actualizează proprietățile
                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description; // Actualizeaza si Description

                    await context.SaveChangesAsync();
                }
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
                    // Debug.WriteLine($\"Categoria cu ID: {categoryId} stearsa.\"); // Optional
                }
            }
        }
        
        // Aici poti adauga si alte metode utile, cum ar fi GetCategoryByIdAsync etc.
    }
}
