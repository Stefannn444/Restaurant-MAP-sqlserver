using Microsoft.EntityFrameworkCore.Design;
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
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RestaurantDbContext>
    {
        public RestaurantDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // folderul proiectului
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>();

            // Dacă vrei să iei conexiunea din App.config, va trebui să citești diferit (mai jos)
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Dacă nu poți citi din appsettings.json, poți pune direct conexiunea aici temporar:
            // var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=RestaurantDb;Trusted_Connection=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new RestaurantDbContext(optionsBuilder.Options);
        }
    }
}
