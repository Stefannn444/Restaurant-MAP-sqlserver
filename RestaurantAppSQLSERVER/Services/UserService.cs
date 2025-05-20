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

namespace RestaurantAppSQLSERVER.Services
{
    public class UserService
    {
        private readonly DbContextFactory _dbContextFactory;

        public UserService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }
        public async Task<User> LoginAsync(string email, string password)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    var users = context.Users
                                       .FromSqlInterpolated($"EXEC AuthenticateUser @Email={email}, @Parola={password}")
                                       .AsEnumerable()
                                       .ToList();
                    var user = users.FirstOrDefault();


                    return user;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling AuthenticateUser stored procedure: {ex.Message}");
                    throw;
                }
            }
        }
        public async Task<bool> RegisterUserAsync(User newUser)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    var resultParam = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    await context.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC RegisterNewUser @Nume={newUser.Nume}, @Prenume={newUser.Prenume}, @Email={newUser.Email}, @Nr_tel={newUser.Nr_tel}, @Adresa={newUser.Adresa}, @Parola={newUser.Parola}, @Rol={(int)UserRole.Client}, @Result={resultParam} OUTPUT"
                    );
                    int registrationResult = (int)resultParam.Value;
                    return registrationResult == 1;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling RegisterNewUser stored procedure: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
