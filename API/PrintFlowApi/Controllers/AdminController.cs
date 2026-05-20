using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Data;
using PrintFlowApi.DTOs;
using PrintFlowApi.Model;
using PrintFlowApi.Services;

namespace PrintFlowApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,Production,Support,Finance")]
public class AdminController(PrintFlowDbContext db, SecurityService securityService) : ControllerBase
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
            .Select(item => new InventoryItemResponse(item.Id, item.Name, item.Category, item.Available, item.Unit, item.Minimum, item.Supplier, item.UnitCost, item.Active))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("estoque/movimentacoes")]
    [Authorize(Roles = "Admin,Production")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateStockMovement(StockMovementRequest request, CancellationToken cancellationToken)
    {
        if (!await securityService.ValidateAdminActionPasswordAsync(request.AdminPassword, cancellationToken))
        {
            return Forbid();
        }

        var item = await db.InventoryItems.FirstOrDefaultAsync(stock => stock.Id == request.InventoryItemId, cancellationToken);
        if (item is null)
        {
            return NotFound(new { message = "Item de estoque nao encontrado." });
        }

        var type = request.Type.Trim().ToLowerInvariant();
        if (type is "in")
        {
            item.Available += request.Quantity;
        }
        else if (type is "out" or "waste")
        {
            item.Available -= request.Quantity;
        }
        else if (type is not "adjustment")
        {
            return BadRequest(new { message = "Tipo de movimentacao invalido." });
        }

        item.UpdatedAt = DateTime.UtcNow;
        db.StockMovements.Add(new StockMovement
        {
            InventoryItemId = item.Id,
            Type = type,
            Quantity = request.Quantity,
            Reason = request.Reason,
            OrderId = request.OrderId,
            CreatedById = User.GetUserId()
        });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Movimentacao registrada." });
    }

    [HttpPut("pedidos/{id:guid}/status")]
    [Authorize(Roles = "Admin,Production,Support")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var sensitive = request.Status is OrderStatus.Cancelled or OrderStatus.Finished;
        if (sensitive && !await securityService.ValidateAdminActionPasswordAsync(request.AdminPassword, cancellationToken))
        {
            return Forbid();
        }

        var order = await db.Orders.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Pedido nao encontrado." });
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;
        order.History.Add(new OrderHistory { Status = request.Status.ToString(), Notes = request.InternalNotes });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Status atualizado." });
    }

    [HttpGet("configuracoes")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<SystemSettingsResponse>> GetSettings(CancellationToken cancellationToken)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        return Ok(ToResponse(settings));
    }

    [HttpPut("configuracoes")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<SystemSettingsResponse>> UpdateSettings(UpdateSystemSettingsRequest request, CancellationToken cancellationToken)
    {
        if (!await securityService.ValidateAdminActionPasswordAsync(request.CurrentAdminPassword, cancellationToken))
        {
            return Forbid();
        }

        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        settings.CompanyName = request.CompanyName.Trim();
        settings.CompanyEmail = request.CompanyEmail.Trim();
        settings.CompanyPhone = request.CompanyPhone.Trim();
        settings.RequireAdminPasswordForSensitiveActions = request.RequireAdminPasswordForSensitiveActions;
        settings.AutoStockDeductionEnabled = request.AutoStockDeductionEnabled;
        settings.StockDeductionTriggerStatus = request.StockDeductionTriggerStatus;
        settings.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.AdminActionPassword))
        {
            settings.AdminActionPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminActionPassword);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(settings));
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

    private async Task<SystemSettings> GetOrCreateSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await db.SystemSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        settings = new SystemSettings();
        db.SystemSettings.Add(settings);
        await db.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private static SystemSettingsResponse ToResponse(SystemSettings settings)
    {
        return new SystemSettingsResponse(
            settings.Id,
            settings.CompanyName,
            settings.CompanyEmail,
            settings.CompanyPhone,
            settings.RequireAdminPasswordForSensitiveActions,
            !string.IsNullOrWhiteSpace(settings.AdminActionPasswordHash),
            settings.AutoStockDeductionEnabled,
            settings.StockDeductionTriggerStatus.ToString());
    }
}
