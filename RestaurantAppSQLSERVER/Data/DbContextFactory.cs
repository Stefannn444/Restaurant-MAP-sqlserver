using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantAppSQLSERVER.Data
{
    public class DbContextFactory
    {
        private readonly string _connectionString;

        public DbContextFactory()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(_connectionString))
            {
                _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=RestaurantDb;Trusted_Connection=True;";
            }
        }

        public RestaurantDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            return new RestaurantDbContext(optionsBuilder.Options);
        }
    }
}
