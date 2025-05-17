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
    public class UserService
    {
        private readonly DbContextFactory _dbContextFactory;

        // Constructor care primește DbContextFactory
        // Acesta rezolvă eroarea "UserService does not contain a constructor that takes 1 arguments"
        public UserService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        // Metodă pentru autentificare (async)
        // Acesta rezolvă eroarea "UserService does not contain a definition for 'LoginAsync'"
        public async Task<User> LoginAsync(string email, string password)
        {
            // Folosește factory-ul pentru a crea un context nou pentru fiecare operație
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Caută utilizatorul după email și parolă (fără hashing, conform cerinței proiectului școlar)
                // Într-o aplicație reală, ai hash-ui parola și ai compara hash-urile în siguranță.
                var user = await context.Users
                                        .FirstOrDefaultAsync(u => u.Email == email && u.Parola == password);

                return user; // Va fi null dacă nu se găsește utilizatorul sau parola e greșită
            }
        }

        // Aici vei adăuga metode pentru înregistrare, etc.
        // Exemplu schelet pentru metoda de înregistrare:
        /*
        public async Task<bool> RegisterUserAsync(User newUser)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Verifică dacă email-ul există deja
                var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);
                if (existingUser != null)
                {
                    return false; // Utilizatorul există deja
                }

                // Adaugă utilizatorul nou în baza de date
                context.Users.Add(newUser);
                await context.SaveChangesAsync();
                return true; // Înregistrare reușită
            }
        }
        */
    }
}
