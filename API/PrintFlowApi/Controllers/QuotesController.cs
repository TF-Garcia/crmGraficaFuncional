using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Data;
using PrintFlowApi.DTOs;
using PrintFlowApi.Model;
using PrintFlowApi.Services;

namespace PrintFlowApi.Controllers;

[ApiController]
[Route("api/orcamentos")]
public class QuotesController(PrintFlowDbContext db, QuoteService quoteService) : ControllerBase
{
    [HttpPost("calcular")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<QuoteResponse>> Calculate(QuoteRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .Include(item => item.Options)
            .FirstOrDefaultAsync(item => item.Id == request.ProductId && item.Active, cancellationToken);

        if (product is null)
        {
            return NotFound(new { message = "Produto nao encontrado." });
        }

        try
        {
            return Ok(quoteService.Calculate(product, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("meus")]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<IReadOnlyList<QuoteSavedResponse>>> MyQuotes(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var quotes = await db.Quotes
            .AsNoTracking()
            .Include(item => item.Product)
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(quotes.Select(ToResponse).ToList());
    }

    [Authorize]
    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<QuoteSavedResponse>> SaveQuote(CreateQuoteRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(item => item.Options)
            .FirstOrDefaultAsync(item => item.Id == request.ProductId && item.Active, cancellationToken);

        if (product is null)
        {
            return NotFound(new { message = "Produto nao encontrado." });
        }

        var quote = quoteService.Calculate(product, new QuoteRequest(
            request.ProductId,
            request.Quantity,
            request.Size,
            request.Material,
            request.PrintMode,
            request.Finishing,
            request.Urgency,
            request.Delivery));

        var entity = new Quote
        {
            Number = await NextQuoteNumberAsync(cancellationToken),
            UserId = User.GetUserId(),
            ProductId = product.Id,
            Quantity = request.Quantity,
            Size = request.Size,
            Material = request.Material,
            PrintMode = request.PrintMode,
            Finishing = request.Finishing,
            Urgency = request.Urgency,
            DeliveryMode = request.Delivery,
            Status = request.Draft ? QuoteStatus.Draft : QuoteStatus.Saved,
            Subtotal = quote.Subtotal,
            UrgencyFee = quote.UrgencyFee,
            DeliveryFee = quote.DeliveryFee,
            Total = quote.Total,
            EstimatedDays = quote.EstimatedDays,
            Notes = request.Notes
        };

        db.Quotes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        entity.Product = product;
        return Ok(ToResponse(entity));
    }

    [Authorize]
    [HttpPost("{id:guid}/converter")]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<OrderResponse>> ConvertToOrder(Guid id, ConvertQuoteRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var quote = await db.Quotes.Include(item => item.Product).FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);
        if (quote is null || quote.Product is null)
        {
            return NotFound(new { message = "Orcamento nao encontrado." });
        }

        if (quote.Status == QuoteStatus.ConvertedToOrder)
        {
            return BadRequest(new { message = "Este orcamento ja foi convertido em pedido." });
        }

        var order = new Order
        {
            Number = await NextOrderNumberAsync(cancellationToken),
            UserId = userId,
            ProductId = quote.ProductId,
            Quantity = quote.Quantity,
            Size = quote.Size,
            Material = quote.Material,
            PrintMode = quote.PrintMode,
            Finishing = quote.Finishing,
            Urgency = quote.Urgency,
            DeliveryMode = quote.DeliveryMode,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = request.PaymentMethod == PaymentMethod.Pickup ? PaymentStatus.CounterPayment : PaymentStatus.Pending,
            Status = request.PaymentMethod == PaymentMethod.Pickup ? OrderStatus.WaitingArtwork : OrderStatus.WaitingPayment,
            Subtotal = quote.Subtotal,
            UrgencyFee = quote.UrgencyFee,
            DeliveryFee = quote.DeliveryFee,
            Total = quote.Total,
            EstimatedDays = quote.EstimatedDays,
            Deadline = DateTime.UtcNow.Date.AddDays(quote.EstimatedDays),
            Notes = quote.Notes
        };
        order.Payment = new Payment { Method = order.PaymentMethod, Status = order.PaymentStatus, Amount = order.Total };
        if (!string.IsNullOrWhiteSpace(request.ArtworkFileName))
        {
            order.Files.Add(new OrderFile { FileName = request.ArtworkFileName.Trim() });
        }

        quote.Status = QuoteStatus.ConvertedToOrder;
        quote.ConvertedOrderId = order.Id;
        quote.UpdatedAt = DateTime.UtcNow;
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        order.Product = quote.Product;
        order.User = await db.Users.FindAsync([userId], cancellationToken);
        return Ok(OrdersController.ToResponse(order));
    }

    private async Task<string> NextQuoteNumberAsync(CancellationToken cancellationToken)
    {
        var count = await db.Quotes.CountAsync(cancellationToken);
        return $"ORC-{1000 + count + 1}";
    }

    private async Task<string> NextOrderNumberAsync(CancellationToken cancellationToken)
    {
        var count = await db.Orders.CountAsync(cancellationToken);
        return (1450 + count + 1).ToString();
    }

    private static QuoteSavedResponse ToResponse(Quote quote)
    {
        return new QuoteSavedResponse(
            quote.Id,
            quote.Number,
            quote.Product?.Name ?? string.Empty,
            quote.Quantity,
            quote.Status.ToString(),
            quote.Total,
            quote.EstimatedDays,
            quote.ExpiresAt,
            quote.CreatedAt);
    }
}
