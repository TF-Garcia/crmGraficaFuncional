using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Data;
using PrintFlowApi.DTOs;
using PrintFlowApi.Model;

namespace PrintFlowApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,Production,Support,Finance")]
public class AdminController(PrintFlowDbContext db) : ControllerBase
{
    [HttpGet("pedidos")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> Orders(CancellationToken cancellationToken)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .Include(order => order.Product)
            .Include(order => order.Payment)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(orders.Select(OrdersController.ToResponse).ToList());
    }

    [HttpGet("clientes")]
    [Authorize(Roles = "Admin,Support,Finance")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<object>> Customers(CancellationToken cancellationToken)
    {
        var customers = await db.Users
            .AsNoTracking()
            .Where(user => user.Role == UserRole.Client)
            .Select(user => new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Phone,
                user.Document,
                user.Address,
                user.Active,
                TotalSpent = user.Orders.Where(order => order.PaymentStatus == PaymentStatus.Paid).Sum(order => order.Total)
            })
            .OrderBy(user => user.Name)
            .ToListAsync(cancellationToken);

        return Ok(customers);
    }

    [HttpGet("estoque")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<IReadOnlyList<InventoryItemResponse>>> Inventory(CancellationToken cancellationToken)
    {
        var items = await db.InventoryItems
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .Select(item => new InventoryItemResponse(item.Id, item.Name, item.Category, item.Available, item.Unit, item.Minimum, item.Supplier, item.UnitCost))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("dashboard")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<object>> Dashboard(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var orders = db.Orders.AsNoTracking();

        return Ok(new
        {
            OrdersToday = await orders.CountAsync(order => order.CreatedAt.Date == now.Date, cancellationToken),
            InProduction = await orders.CountAsync(order => order.Status == OrderStatus.InProduction, cancellationToken),
            WaitingPayment = await orders.CountAsync(order => order.Status == OrderStatus.WaitingPayment, cancellationToken),
            RevenueMonth = await orders.Where(order => order.CreatedAt >= monthStart && order.PaymentStatus == PaymentStatus.Paid).SumAsync(order => order.Total, cancellationToken),
            LowInventory = await db.InventoryItems.CountAsync(item => item.Available < item.Minimum, cancellationToken)
        });
    }
}
