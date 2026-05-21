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
[Route("api/pedidos")]
[Authorize]
public class OrdersController(PrintFlowDbContext db, QuoteService quoteService) : ControllerBase
{
    [HttpGet("meus")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> MyOrders(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var orders = await BaseOrderQuery()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(orders.Select(ToResponse).ToList());
    }

    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(item => item.Options)
            .FirstOrDefaultAsync(item => item.Id == request.ProductId && item.Active, cancellationToken);

        if (product is null)
        {
            return NotFound(new { message = "Produto nao encontrado." });
        }

        if (request.PaymentMethod == PaymentMethod.Pickup && !product.AllowPickupPayment)
        {
            return BadRequest(new { message = "Este produto exige pagamento antecipado." });
        }

        QuoteResponse quote;
        try
        {
            quote = quoteService.Calculate(product, new QuoteRequest(
                request.ProductId,
                request.Quantity,
                request.Size,
                request.Material,
                request.PrintMode,
                request.Finishing,
                request.Urgency,
                request.Delivery));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var order = new Order
        {
            Number = await NextOrderNumberAsync(cancellationToken),
            UserId = User.GetUserId(),
            ProductId = product.Id,
            Quantity = request.Quantity,
            Size = request.Size,
            Material = request.Material,
            PrintMode = request.PrintMode,
            Finishing = request.Finishing,
            Urgency = request.Urgency,
            DeliveryMode = request.Delivery,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = request.PaymentMethod == PaymentMethod.Pickup ? PaymentStatus.Paid : PaymentStatus.Pending,
            Status = request.PaymentMethod == PaymentMethod.Pickup ? OrderStatus.PaymentConfirmed : OrderStatus.WaitingPayment,
            Subtotal = quote.Subtotal,
            UrgencyFee = quote.UrgencyFee,
            DeliveryFee = quote.DeliveryFee,
            Total = quote.Total,
            EstimatedDays = quote.EstimatedDays,
            Deadline = DateTime.UtcNow.Date.AddDays(quote.EstimatedDays),
            Notes = request.Notes
        };

        order.Payment = new Payment
        {
            Method = request.PaymentMethod,
            Status = order.PaymentStatus,
            Amount = order.Total,
            Provider = request.PaymentMethod == PaymentMethod.Pickup ? "manual" : "mercado-pago"
        };
        order.History.Add(new OrderHistory { Status = "Pedido criado", Notes = "Pedido aberto pelo cliente." });

        if (!string.IsNullOrWhiteSpace(request.ArtworkFileName))
        {
            order.Files.Add(new OrderFile { FileName = request.ArtworkFileName.Trim() });
            order.History.Add(new OrderHistory { Status = "Arte enviada", Notes = request.ArtworkFileName.Trim() });
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        var created = await BaseOrderQuery().FirstAsync(item => item.Id == order.Id, cancellationToken);
        return CreatedAtAction(nameof(MyOrders), new { id = order.Id }, ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<OrderResponse>> Update(Guid id, CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var settings = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (settings is null || !settings.AllowCustomerOrderEdit)
        {
            return Forbid();
        }

        var userId = User.GetUserId();
        var order = await db.Orders
            .Include(item => item.Product)
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (order is null)
        {
            return NotFound(new { message = "Pedido nao encontrado." });
        }

        if (order.Status is OrderStatus.InProduction or OrderStatus.Finishing or OrderStatus.ReadyForPickup or OrderStatus.OutForDelivery or OrderStatus.Finished or OrderStatus.Cancelled)
        {
            return BadRequest(new { message = "Este pedido nao pode mais ser editado." });
        }

        var product = await db.Products.Include(item => item.Options).FirstOrDefaultAsync(item => item.Id == request.ProductId && item.Active, cancellationToken);
        if (product is null)
        {
            return NotFound(new { message = "Produto nao encontrado." });
        }

        if (request.PaymentMethod == PaymentMethod.Pickup && !product.AllowPickupPayment)
        {
            return BadRequest(new { message = "Este produto exige pagamento antecipado." });
        }

        QuoteResponse quote;
        try
        {
            quote = quoteService.Calculate(product, new QuoteRequest(request.ProductId, request.Quantity, request.Size, request.Material, request.PrintMode, request.Finishing, request.Urgency, request.Delivery));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        order.ProductId = product.Id;
        order.Quantity = request.Quantity;
        order.Size = request.Size;
        order.Material = request.Material;
        order.PrintMode = request.PrintMode;
        order.Finishing = request.Finishing;
        order.Urgency = request.Urgency;
        order.DeliveryMode = request.Delivery;
        order.PaymentMethod = request.PaymentMethod;
        order.PaymentStatus = request.PaymentMethod == PaymentMethod.Pickup ? PaymentStatus.Paid : PaymentStatus.Pending;
        order.Status = request.PaymentMethod == PaymentMethod.Pickup ? OrderStatus.PaymentConfirmed : OrderStatus.WaitingPayment;
        order.Subtotal = quote.Subtotal;
        order.UrgencyFee = quote.UrgencyFee;
        order.DeliveryFee = quote.DeliveryFee;
        order.Total = quote.Total;
        order.EstimatedDays = quote.EstimatedDays;
        order.Deadline = DateTime.UtcNow.Date.AddDays(quote.EstimatedDays);
        order.Notes = request.Notes;
        order.UpdatedAt = DateTime.UtcNow;
        order.Payment ??= new Payment { OrderId = order.Id };
        order.Payment.Method = order.PaymentMethod;
        order.Payment.Status = order.PaymentStatus;
        order.Payment.Amount = order.Total;
        order.Payment.Provider = request.PaymentMethod == PaymentMethod.Pickup ? "manual" : "mercado-pago";
        order.Payment.UpdatedAt = DateTime.UtcNow;
        order.History.Add(new OrderHistory { Status = "Pedido editado", Notes = "Pedido alterado pelo cliente." });

        await db.SaveChangesAsync(cancellationToken);
        var updated = await BaseOrderQuery().FirstAsync(item => item.Id == id, cancellationToken);
        return Ok(ToResponse(updated));
    }

    [HttpPost("{id:guid}/cancelar")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var settings = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (settings is null || !settings.AllowCustomerOrderCancellation)
        {
            return Forbid();
        }

        var order = await db.Orders.FirstOrDefaultAsync(item => item.Id == id && item.UserId == User.GetUserId(), cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Pedido nao encontrado." });
        }

        if (order.Status is OrderStatus.InProduction or OrderStatus.Finishing or OrderStatus.ReadyForPickup or OrderStatus.OutForDelivery or OrderStatus.Finished)
        {
            return BadRequest(new { message = "Este pedido nao pode mais ser cancelado pelo cliente." });
        }

        order.Status = OrderStatus.Cancelled;
        order.PaymentStatus = order.PaymentStatus == PaymentStatus.Paid ? PaymentStatus.Refunded : PaymentStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        order.History.Add(new OrderHistory { Status = "Cancelado pelo cliente", Notes = "Cancelamento solicitado pelo painel do cliente." });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Pedido cancelado." });
    }

    [HttpPost("{id:guid}/estorno")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Refund(Guid id, CancellationToken cancellationToken)
    {
        var settings = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (settings is null || !settings.AllowCustomerRefundRequest)
        {
            return Forbid();
        }

        var order = await db.Orders.Include(item => item.Payment).FirstOrDefaultAsync(item => item.Id == id && item.UserId == User.GetUserId(), cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Pedido nao encontrado." });
        }

        if (order.PaymentStatus != PaymentStatus.Paid)
        {
            return BadRequest(new { message = "Apenas pedidos pagos podem receber estorno." });
        }

        order.PaymentStatus = PaymentStatus.Refunded;
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        if (order.Payment is not null)
        {
            order.Payment.Status = PaymentStatus.Refunded;
            order.Payment.UpdatedAt = DateTime.UtcNow;
        }
        order.History.Add(new OrderHistory { Status = "Estorno solicitado", Notes = "Estorno solicitado pelo cliente." });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Estorno solicitado." });
    }

    private IQueryable<Order> BaseOrderQuery()
    {
        return db.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .Include(order => order.Product)
            .Include(order => order.Payment);
    }

    private async Task<string> NextOrderNumberAsync(CancellationToken cancellationToken)
    {
        var count = await db.Orders.CountAsync(cancellationToken);
        return (1450 + count + 1).ToString();
    }

    public static OrderResponse ToResponse(Order order)
    {
        var paymentStatus = order.PaymentMethod == PaymentMethod.Pickup && order.PaymentStatus == PaymentStatus.CounterPayment
            ? PaymentStatus.Paid.ToString()
            : order.PaymentStatus.ToString();

        return new OrderResponse(
            order.Id,
            order.Number,
            order.User?.Name ?? string.Empty,
            order.ProductId,
            order.Product?.Name ?? string.Empty,
            order.Quantity,
            order.Size,
            order.Material,
            order.PrintMode,
            order.Finishing,
            order.Urgency,
            order.DeliveryMode.ToString(),
            order.Status.ToString(),
            paymentStatus,
            order.PaymentMethod.ToString(),
            order.Total,
            order.Deadline,
            order.CreatedAt,
            order.Notes);
    }
}
