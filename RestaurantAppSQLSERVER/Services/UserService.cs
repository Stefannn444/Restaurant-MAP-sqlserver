using Microsoft.EntityFrameworkCore;
using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient; // NECESAR pentru SqlParameter
using System.Data; // NECESAR pentru ParameterDirection

namespace RestaurantAppSQLSERVER.Services
{
    public class UserService
    {
        private readonly DbContextFactory _dbContextFactory;

        public UserService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        // Metodă pentru autentificare (async) folosind Procedura Stocata AuthenticateUser
        public async Task<User> LoginAsync(string email, string password)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    // Apeleaza procedura stocata AuthenticateUser
                    // Folosim FromSqlInterpolated pentru a pasa parametrii in siguranta (previne SQL Injection)
                    var users = context.Users
                                       .FromSqlInterpolated($"EXEC AuthenticateUser @Email={email}, @Parola={password}")
                                       // Adauga .AsEnumerable() aici pentru a executa SP-ul si a aduce rezultatele in memorie
                                       .AsEnumerable() // Executa query-ul pe server si aduce rezultatele in client
                                                       // Acum folosim ToList() (sincron) pe colectia din memorie
                                       .ToList(); // Executa AsEnumerable() si returneaza o lista in memorie

                    // Acum aplica FirstOrDefault() pe colectia din memorie
                    var user = users.FirstOrDefault();


                    return user; // Va fi null daca procedura nu returneaza niciun rand
                }
                catch (Exception ex)
                {
                    // Gestioneaza erorile care pot aparea la apelarea procedurii stocate
                    System.Diagnostics.Debug.WriteLine($"Error calling AuthenticateUser stored procedure: {ex.Message}");
                    // Arunca exceptia mai departe sau returneaza null/un indicator de eroare
                    throw; // Sau return null; in functie de cum vrei sa gestionezi in ViewModel
                }
            }
        }

        // Metoda de înregistrare (async) folosind Procedura Stocata RegisterNewUser cu parametru OUTPUT
        public async Task<bool> RegisterUserAsync(User newUser)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    // Declara parametrul de output pentru rezultat
                    // Trebuie sa specifici tipul SQL si directia (Output)
                    var resultParam = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };

                    // Apeleaza procedura stocata folosind ExecuteSqlInterpolatedAsync
                    // Pasam parametrii de intrare si parametrul de output
                    // Adaugam "OUTPUT" la finalul parametrului in string-ul interpolat
                    await context.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC RegisterNewUser @Nume={newUser.Nume}, @Prenume={newUser.Prenume}, @Email={newUser.Email}, @Nr_tel={newUser.Nr_tel}, @Adresa={newUser.Adresa}, @Parola={newUser.Parola}, @Rol={(int)UserRole.Client}, @Result={resultParam} OUTPUT"
                    );

                    // Dupa executie, valoarea parametrului de output este disponibila in resultParam.Value
                    // Convertim valoarea la int
                    int registrationResult = (int)resultParam.Value;

                    // Interpretam rezultatul procedurii stocate
                    return registrationResult == 1; // Returneaza true daca procedura a indicat succesul (1)
                }
                catch (Exception ex)
                {
                    // Gestioneaza erorile care pot aparea la apelarea procedurii stocate
                    System.Diagnostics.Debug.WriteLine($"Error calling RegisterNewUser stored procedure: {ex.Message}");
                    // Poate verifica tipul exceptiei pentru a detecta, de exemplu, o incalcare a constrangerii de unicitate (desi SP-ul ar trebui sa o gestioneze)
                    return false; // Inregistrare esuata in caz de exceptie
                }
            }
        }

        // Aici poti adauga si alte metode utile
    }
}
