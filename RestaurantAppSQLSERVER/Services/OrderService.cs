using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Microsoft.Data.SqlClient; // Necesara pentru SqlParameter si SqlDbType
using System.Data; // Necesara pentru DataTable, SqlDbType, ParameterDirection
// using RestaurantAppSQLSERVER.Models.Wrappers; // Poate nu mai este necesara daca nu folosesti ClientOrderItemHistory


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

        // Metoda pentru a obtine toate comenzile (poate fi inlocuita cu SP complexa GetEmployeeOrders)
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                // Include User pentru a afisa informatii despre client
                // Include OrderItems
                return await context.Orders
                                    .Include(o => o.User) // Include userul care a plasat comanda
                                    .Include(o => o.OrderItems) // Include itemii din comanda
                                    .OrderByDescending(o => o.OrderDate) // Afisam cele mai recente comenzi primele
                                    .ToListAsync();
            }
        }

        // Metoda pentru a obtine o comanda dupa ID cu toate detaliile (poate fi inlocuita cu SP complexa)
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Orders
                                    .Include(o => o.User)
                                    .Include(o => o.OrderItems)
                                    .FirstOrDefaultAsync(o => o.Id == orderId);
            }
        }

        // --- Metoda pentru a plasa o comanda folosind Procedura Stocata PlaceOrder ---
        // Primeste ID-ul utilizatorului, costurile calculate si lista de itemi din cos
        // Returneaza un obiect custom cu rezultatul (succes/esec, ID comanda, mesaj)
        public async Task<PlaceOrderResult> PlaceOrderAsync(int userId, decimal discountAmount, decimal transportCost, List<OrderItemData> orderItems)
        {
            // Valideaza input-ul (optional, dar recomandat)
            if (orderItems == null || !orderItems.Any())
            {
                return new PlaceOrderResult { IsSuccess = false, ResultCode = -3, Message = "Cosul de cumparaturi este gol." };
            }

            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    // 1. Construieste Table-Valued Parameter-ul (DataTable)
                    // Numele coloanelor si tipurile de date trebuie sa se potriveasca EXACT cu OrderItemType din SQL Server
                    var orderItemsDataTable = new DataTable();
                    orderItemsDataTable.Columns.Add("ItemId", typeof(int));
                    orderItemsDataTable.Columns.Add("ItemType", typeof(string));
                    orderItemsDataTable.Columns.Add("Quantity", typeof(int));
                    orderItemsDataTable.Columns.Add("UnitPrice", typeof(decimal));
                    orderItemsDataTable.Columns.Add("ItemName", typeof(string));

                    // Populeaza DataTable cu datele din lista de itemi primita
                    foreach (var item in orderItems)
                    {
                        orderItemsDataTable.Rows.Add(item.ItemId, item.ItemType, item.Quantity, item.UnitPrice, item.ItemName);
                    }

                    // 2. Declara Parametrii pentru Procedura Stocata
                    var userIdParam = new SqlParameter("@UserId", userId);
                    var discountAmountParam = new SqlParameter("@DiscountAmount", discountAmount);
                    var transportCostParam = new SqlParameter("@TransportCost", transportCost);

                    // Parametrul pentru Table-Valued Parameter
                    // Specifica SqlDbType.Structured si numele tipului tabelar din baza de date ("OrderItemType")
                    var orderItemsParam = new SqlParameter("@OrderItems", orderItemsDataTable)
                    {
                        TypeName = "OrderItemType", // << Numele tipului tabelar din SQL Server
                        SqlDbType = SqlDbType.Structured // << Indica ca este un tip tabelar
                    };

                    // Parametri de output
                    var newOrderIdParam = new SqlParameter("@NewOrderId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var resultMessageParam = new SqlParameter("@ResultMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output }; // -1 pentru NVARCHAR(MAX)


                    // 3. Apeleaza Procedura Stocata
                    // Folosim ExecuteSqlRawAsync pentru ca pasam parametrii ca obiecte SqlParameter
                    // Include toti parametrii in string-ul de apel, marcand parametrii de output cu "OUTPUT"
                    await context.Database.ExecuteSqlRawAsync(
                        "EXEC PlaceOrder @UserId, @DiscountAmount, @TransportCost, @OrderItems, @NewOrderId OUTPUT, @ResultCode OUTPUT, @ResultMessage OUTPUT",
                        userIdParam,
                        discountAmountParam,
                        transportCostParam,
                        orderItemsParam,
                        newOrderIdParam,
                        resultCodeParam,
                        resultMessageParam
                    );

                    // 4. Citeste Parametrii de Output si Returneaza Rezultatul
                    int resultCode = (int)resultCodeParam.Value;
                    string resultMessage = resultMessageParam.Value.ToString();
                    int? newOrderId = (newOrderIdParam.Value != DBNull.Value) ? (int)newOrderIdParam.Value : (int?)null;


                    return new PlaceOrderResult
                    {
                        IsSuccess = (resultCode == 1), // Succes daca ResultCode este 1
                        ResultCode = resultCode,
                        Message = resultMessage,
                        OrderId = newOrderId
                    };

                }
                catch (Exception ex)
                {
                    // Gestioneaza erorile care pot aparea la apelarea procedurii stocata
                    Debug.WriteLine($"Error calling PlaceOrder stored procedure: {ex.Message}");
                    // Arunca exceptia mai departe sau returneaza un indicator de eroare
                    return new PlaceOrderResult { IsSuccess = false, ResultCode = -2, Message = $"Eroare la plasarea comenzii: {ex.Message}" };
                }
            }
        }

        // Metoda pentru a obtine istoricul comenzilor unui client folosind EF Core
        // Returneaza o lista de obiecte Order cu OrderItems incluse
        public async Task<List<Order>> GetClientOrdersAsync(int userId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    // Folosim LINQ si Include pentru a prelua comenzile utilizatorului si itemii lor
                    var clientOrders = await context.Orders
                                                    .Where(o => o.UserId == userId) // Filtreaza dupa ID-ul utilizatorului
                                                    .Include(o => o.OrderItems) // Include itemii din comanda
                                                    .OrderByDescending(o => o.OrderDate) // Ordoneaza de la cele mai recente la cele mai vechi
                                                    .ToListAsync(); // Executa query-ul si aduce rezultatele

                    return clientOrders;
                }
                catch (Exception ex)
                {
                    // Gestioneaza erorile
                    Debug.WriteLine($"Error retrieving client orders with EF Core: {ex.Message}");
                    throw; // Arunca exceptia mai departe
                }
            }
        }

        // --- NOU: Metoda pentru a numara comenzile unui utilizator intr-un interval de timp ---
        // Aceasta metoda va fi folosita pentru logica de reducere de loialitate
        public async Task<int> GetOrderCountInTimeFrameAsync(int userId, int timeIntervalDays)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    // Calculeaza data de inceput a intervalului (acum - timeIntervalDays)
                    var startDate = DateTime.Now.AddDays(-timeIntervalDays);

                    // Conteaza comenzile utilizatorului plasate incepand cu startDate
                    var orderCount = await context.Orders
                                                  .Where(o => o.UserId == userId && o.OrderDate >= startDate)
                                                  .CountAsync(); // Conteaza numarul de randuri returnate

                    return orderCount;
                }
                catch (Exception ex)
                {
                    // Gestioneaza erorile
                    Debug.WriteLine($"Error counting user orders in time frame: {ex.Message}");
                    throw; // Arunca exceptia mai departe
                }
            }
        }


        // Metoda pentru a actualiza o comanda existenta (probabil folosita de angajati)
        // Poate fi inlocuita cu SP complexa UpdateOrderStatus sau similar
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
                // Exclude OrderItems de la actualizarea directa prin SetValues
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

        // Metoda pentru a sterge o comanda (poate fi inlocuita cu SP simpla DeleteOrder)
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

        public async Task<CancelOrderResult> CancelOrderAsync(int orderId, int userId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    // Cauta comanda dupa ID si verifica daca apartine utilizatorului
                    var orderToCancel = await context.Orders
                                                     .Where(o => o.Id == orderId && o.UserId == userId)
                                                     .FirstOrDefaultAsync();

                    // Verifica conditiile pentru anulare
                    if (orderToCancel == null)
                    {
                        // Comanda nu a fost gasita sau nu apartine utilizatorului
                        return new CancelOrderResult { IsSuccess = false, Message = "Comanda nu a fost gasita sau nu aveti permisiunea de a o anula." };
                    }

                    if (orderToCancel.Status != "Inregistrata"&&orderToCancel.Status!="0") // Compara cu starea "Inregistrata"
                    {
                        // Starea comenzii nu permite anularea
                        return new CancelOrderResult { IsSuccess = false, Message = $"Comanda poate fi anulata doar daca este in starea \"Inregistrata\". Stare curenta: {orderToCancel.Status}" };
                    }

                    // Toate conditiile sunt indeplinite, actualizeaza starea comenzii la 'Anulata'
                    orderToCancel.Status = "Anulata";

                    // Salveaza modificarile in baza de date
                    await context.SaveChangesAsync();

                    // Returneaza rezultatul la succes
                    return new CancelOrderResult { IsSuccess = true, Message = "Comanda a fost anulata cu succes." };
                }
                catch (Exception ex)
                {
                    // Gestioneaza erorile care pot aparea la accesarea bazei de date
                    Debug.WriteLine($"Error cancelling order with EF Core: {ex.Message}");
                    return new CancelOrderResult { IsSuccess = false, Message = $"A aparut o eroare la anularea comenzii: {ex.Message}" };
                }
            }
        }
       

        // Metoda helper pentru a genera un cod unic de comanda (exemplu simplificat)
        // Aceasta logica este acum in SP PlaceOrder, deci aceasta metoda nu mai este necesara aici pentru plasarea comenzii
        /*
        private string GenerateUniqueOrderCode()
        {
            // Implementeaza o logica reala de generare cod unic (ex: data + counter, GUID partial etc.)
            // Verifica in baza de date sa te asiguri ca este unic.
            return $"ORD-{DateTime.Now.ToString("yyyyMMddHHmmss")}-{Guid.NewGuid().ToString().Substring(0, 4)}";
        }
        */

        // Aici poti adauga si alte metode utile, ex: GetOrdersByStatusAsync, GetOrdersByDateRangeAsync etc.
    }

    // Clasa helper pentru a pasa datele unui item din cos catre OrderService/SP
    public class OrderItemData
    {
        public int ItemId { get; set; }
        public string ItemType { get; set; } // "Dish" sau "MenuItem"
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Pretul unitar la momentul adaugarii in cos
        public string ItemName { get; set; } // Numele la momentul adaugarii in cos
    }

    // Clasa helper pentru a returna rezultatul plasarii comenzii
    public class PlaceOrderResult
    {
        public bool IsSuccess { get; set; }
        public int ResultCode { get; set; } // 1 succes, 0 stoc insuficient, -1 eroare necunoscuta, -2 eroare SQL, -3 cos gol
        public string Message { get; set; }
        public int? OrderId { get; set; } // ID-ul comenzii nou create (null in caz de esec)
        // Poti adauga si OrderCode aici daca vrei sa-l returnezi
    }
}

public class CancelOrderResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}