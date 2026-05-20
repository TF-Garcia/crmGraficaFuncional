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
            PaymentStatus = request.PaymentMethod == PaymentMethod.Pickup ? PaymentStatus.PendingPickup : PaymentStatus.Pending,
            Status = request.PaymentMethod == PaymentMethod.Pickup ? OrderStatus.WaitingArtwork : OrderStatus.WaitingPayment,
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
        return new OrderResponse(
            order.Id,
            order.Number,
            order.User?.Name ?? string.Empty,
            order.Product?.Name ?? string.Empty,
            order.Quantity,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.PaymentMethod.ToString(),
            order.Total,
            order.Deadline,
            order.Payment?.CheckoutUrl,
            order.CreatedAt);
    }
}
