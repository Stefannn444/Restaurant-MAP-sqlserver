using RestaurantAppSQLSERVER.Data;
using RestaurantAppSQLSERVER.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;


namespace RestaurantAppSQLSERVER.Services
{
    public class OrderService
    {
        private readonly DbContextFactory _dbContextFactory;

        public OrderService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return await context.Orders
                                    .Include(o => o.User)
                                    .Include(o => o.OrderItems)
                                    .OrderByDescending(o => o.OrderDate)
                                    .ToListAsync();
            }
        }
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
        public async Task<PlaceOrderResult> PlaceOrderAsync(int userId, decimal discountAmount, decimal transportCost, List<OrderItemData> orderItems)
        {
            if (orderItems == null || !orderItems.Any())
            {
                return new PlaceOrderResult { IsSuccess = false, ResultCode = -3, Message = "Cosul de cumparaturi este gol." };
            }

            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    var orderItemsDataTable = new DataTable();
                    orderItemsDataTable.Columns.Add("ItemId", typeof(int));
                    orderItemsDataTable.Columns.Add("ItemType", typeof(string));
                    orderItemsDataTable.Columns.Add("Quantity", typeof(int));
                    orderItemsDataTable.Columns.Add("UnitPrice", typeof(decimal));
                    orderItemsDataTable.Columns.Add("ItemName", typeof(string));
                    foreach (var item in orderItems)
                    {
                        orderItemsDataTable.Rows.Add(item.ItemId, item.ItemType, item.Quantity, item.UnitPrice, item.ItemName);
                    }
                    var userIdParam = new SqlParameter("@UserId", userId);
                    var discountAmountParam = new SqlParameter("@DiscountAmount", discountAmount);
                    var transportCostParam = new SqlParameter("@TransportCost", transportCost);
                    var orderItemsParam = new SqlParameter("@OrderItems", orderItemsDataTable)
                    {
                        TypeName = "OrderItemType",
                        SqlDbType = SqlDbType.Structured
                    };
                    var newOrderIdParam = new SqlParameter("@NewOrderId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var resultMessageParam = new SqlParameter("@ResultMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
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
                    int resultCode = (int)resultCodeParam.Value;
                    string resultMessage = resultMessageParam.Value.ToString();
                    int? newOrderId = (newOrderIdParam.Value != DBNull.Value) ? (int)newOrderIdParam.Value : (int?)null;


                    return new PlaceOrderResult
                    {
                        IsSuccess = (resultCode == 1),
                        ResultCode = resultCode,
                        Message = resultMessage,
                        OrderId = newOrderId
                    };

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error calling PlaceOrder stored procedure: {ex.Message}");
                    return new PlaceOrderResult { IsSuccess = false, ResultCode = -2, Message = $"Eroare la plasarea comenzii: {ex.Message}" };
                }
            }
        }
        public async Task<List<Order>> GetClientOrdersAsync(int userId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    var clientOrders = await context.Orders
                                                    .Where(o => o.UserId == userId)
                                                    .Include(o => o.OrderItems)
                                                    .OrderByDescending(o => o.OrderDate)
                                                    .ToListAsync();

                    return clientOrders;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error retrieving client orders with EF Core: {ex.Message}");
                    throw;
                }
            }
        }
        public async Task<int> GetOrderCountInTimeFrameAsync(int userId, int timeIntervalDays)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    var startDate = DateTime.Now.AddDays(-timeIntervalDays);
                    var orderCount = await context.Orders
                                                  .Where(o => o.UserId == userId && o.OrderDate >= startDate)
                                                  .CountAsync();

                    return orderCount;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error counting user orders in time frame: {ex.Message}");
                    throw;
                }
            }
        }
        public async Task UpdateOrderAsync(Order order)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingOrder = await context.Orders
                                                .Include(o => o.OrderItems)
                                                .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (existingOrder == null)
                {
                    throw new InvalidOperationException($"Comanda cu ID-ul {order.Id} nu a fost gasita.");
                }
                context.Entry(existingOrder).CurrentValues.SetValues(order);


                await context.SaveChangesAsync();
                Debug.WriteLine($"Comanda cu ID: {order.Id} actualizata. Status: {order.Status}");
            }
        }
        public async Task DeleteOrderAsync(int orderId)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var orderToDelete = await context.Orders.FindAsync(orderId);
                if (orderToDelete != null)
                {

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
                    var orderToCancel = await context.Orders
                                                     .Where(o => o.Id == orderId && o.UserId == userId)
                                                     .FirstOrDefaultAsync();
                    if (orderToCancel == null)
                    {
                        return new CancelOrderResult { IsSuccess = false, Message = "Comanda nu a fost gasita sau nu aveti permisiunea de a o anula." };
                    }

                    if (orderToCancel.Status != "Inregistrata"&&orderToCancel.Status!="0")
                    {
                        return new CancelOrderResult { IsSuccess = false, Message = $"Comanda poate fi anulata doar daca este in starea \"Inregistrata\". Stare curenta: {orderToCancel.Status}" };
                    }
                    orderToCancel.Status = "Anulata";
                    await context.SaveChangesAsync();
                    return new CancelOrderResult { IsSuccess = true, Message = "Comanda a fost anulata cu succes." };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cancelling order with EF Core: {ex.Message}");
                    return new CancelOrderResult { IsSuccess = false, Message = $"A aparut o eroare la anularea comenzii: {ex.Message}" };
                }
            }
        }
    }
    public class OrderItemData
    {
        public int ItemId { get; set; }
        public string ItemType { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ItemName { get; set; }
    }
    public class PlaceOrderResult
    {
        public bool IsSuccess { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; }
        public int? OrderId { get; set; }
    }
}

public class CancelOrderResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}