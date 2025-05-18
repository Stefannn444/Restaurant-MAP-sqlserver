using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace RestaurantAppSQLSERVER.Services
{
    // Serviciu pentru gestionarea operatiilor CRUD pe entitatea Order
    public class OrderService
    {
        private readonly DbContextFactory _dbContextFactory;

        public OrderService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        // Metoda pentru a obtine toate comenzile
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Include User pentru a afisa informatii despre client
                // Include OrderItems si entitatile legate (Dish/MenuItem) daca este necesar
                // Nota: In OrderItem, ItemType si ItemId indica Dish sau MenuItem.
                // Pentru a include detaliile complete ale Dish/MenuItem, ar fi nevoie
                // de o logica mai complexa sau de proprietati de navigare conditionale,
                // sau sa le incarci separat in ViewModel.
                // Momentan, ne bazam pe datele istorice stocate in OrderItem (Name, Price).
                return await context.Orders
                                    .Include(o => o.User) // Include userul care a plasat comanda
                                    .Include(o => o.OrderItems) // Include itemii din comanda
                                                                // Daca vrei sa incluzi Dish/MenuItem complete, ar fi complex aici:
                                                                // .Include(o => o.OrderItems).ThenInclude(...) // Nu putem include conditionat Dish SAU MenuItem direct
                                    .OrderByDescending(o => o.OrderDate) // Afisam cele mai recente comenzi primele
                                    .ToListAsync();
            }
        }

        // Metoda pentru a obtine o comanda dupa ID cu toate detaliile
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Orders
                                    .Include(o => o.User)
                                    .Include(o => o.OrderItems)
                                    // Daca vrei sa incluzi Dish/MenuItem complete, ar fi complex aici:
                                    // .Include(o => o.OrderItems).ThenInclude(...)
                                    .FirstOrDefaultAsync(o => o.Id == orderId);
            }
        }

        // Metoda pentru a adauga o comanda noua (probabil folosita de partea de client)
        // Angajatii ar putea adauga comenzi manual, dar fluxul principal e de la client.
        // In dashboard-ul angajatului, focusul e pe vizualizare si actualizare stare.
        public async Task AddOrderAsync(Order order)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Valideaza si pregateste comanda inainte de adaugare
                if (string.IsNullOrWhiteSpace(order.OrderCode))
                {
                    // Genereaza un OrderCode unic daca nu exista (logica mai complexa)
                    order.OrderCode = GenerateUniqueOrderCode(); // Implementeaza aceasta metoda
                }

                // Asigura-te ca User-ul este atasat contextului daca exista
                if (order.User != null && context.Entry(order.User).State == EntityState.Detached)
                {
                    context.Users.Attach(order.User);
                }

                // Asigura-te ca OrderItems si entitatile lor legate (Dish/MenuItem) sunt gestionate corect
                if (order.OrderItems != null)
                {
                    foreach (var orderItem in order.OrderItems)
                    {
                        // Aici, OrderItem contine deja Name, Price, Quantity la momentul comenzii.
                        // Nu este necesar sa atasezi Dish/MenuItem la salvarea OrderItem-ului,
                        // deoarece OrderItem stocheaza datele istorice.
                        // Daca ai avea nevoie sa le atasezi, ar fi complex din cauza ItemType/ItemId.
                    }
                }

                order.OrderDate = DateTime.Now; // Seteaza data comenzii la momentul adaugarii
                // TotalPrice, TransportCost, DiscountPercentage ar trebui calculate inainte de a apela AddOrderAsync

                context.Orders.Add(order);
                await context.SaveChangesAsync();
                Debug.WriteLine($"Comanda '{order.OrderCode}' adaugata cu ID: {order.Id}");
            }
        }

        // Metoda pentru a actualiza o comanda existenta
        // In dashboard-ul angajatului, aceasta va fi folosita in principal pentru a actualiza Starea.
        public async Task UpdateOrderAsync(Order order)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Verifica daca comanda exista in baza de date si include OrderItems
                var existingOrder = await context.Orders
                                                .Include(o => o.OrderItems) // Include OrderItems pentru a le putea gestiona daca este necesar
                                                .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (existingOrder == null)
                {
                    throw new InvalidOperationException($"Comanda cu ID-ul {order.Id} nu a fost gasita.");
                }

                // Actualizeaza proprietatile simple ale comenzii (inclusiv Status, EstimatedDeliveryTime, TotalPrice etc.)
                context.Entry(existingOrder).CurrentValues.SetValues(order);

                // --- Gestionarea OrderItems (daca permiti modificarea itemilor din comanda in dashboard) ---
                // Daca angajatii pot modifica itemii dintr-o comanda existenta, logica ar fi similara
                // cu gestionarea MenuItemDish-urilor in MenuService: compara itemii existenti
                // cu cei noi si adauga/sterge/actualizeaza inregistrarile in tabela OrderItem.
                // Pentru simplitatea CRUD-ului angajatului, ne putem concentra doar pe actualizarea Starii.
                // Daca vrei sa permiti modificarea itemilor, va trebui sa implementezi logica aici.
                // Exemplu (pseudocod):
                // var existingItemIds = existingOrder.OrderItems.Select(oi => oi.Id).ToList();
                // var newItemIds = order.OrderItems.Select(oi => oi.Id).ToList(); // Presupune ca OrderItems noi au Id-uri temporare sau 0
                // ... logica de comparare si adaugare/stergere/actualizare OrderItem-uri ...
                // --- Sfarsit gestionare OrderItems ---


                await context.SaveChangesAsync();
                Debug.WriteLine($"Comanda cu ID: {order.Id} actualizata. Status: {order.Status}");
            }
        }

        // Metoda pentru a sterge o comanda
        public async Task DeleteOrderAsync(int orderId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var orderToDelete = await context.Orders.FindAsync(orderId);
                if (orderToDelete != null)
                {
                    // Daca ai setat OnDelete(DeleteBehavior.Cascade) pentru relatia Order -> OrderItem,
                    // stergerea comenzii va sterge automat OrderItem-urile asociate.

                    context.Orders.Remove(orderToDelete);
                    await context.SaveChangesAsync();
                    Debug.WriteLine($"Comanda cu ID: {orderId} sters.");
                }
            }
        }

        // Metoda helper pentru a genera un cod unic de comanda (exemplu simplificat)
        private string GenerateUniqueOrderCode()
        {
            // Implementeaza o logica reala de generare cod unic (ex: data + counter, GUID partial etc.)
            // Verifica in baza de date sa te asiguri ca este unic.
            return $"ORD-{DateTime.Now.ToString("yyyyMMddHHmmss")}-{Guid.NewGuid().ToString().Substring(0, 4)}";
        }

        // Aici poti adauga si alte metode utile, ex: GetOrdersByStatusAsync, GetOrdersByDateRangeAsync etc.
    }
}
