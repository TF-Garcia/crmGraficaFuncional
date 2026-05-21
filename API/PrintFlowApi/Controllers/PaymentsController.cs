using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PrintFlowApi.Data;
using PrintFlowApi.DTOs;
using PrintFlowApi.Model;
using PrintFlowApi.Services;

namespace PrintFlowApi.Controllers;

[ApiController]
[Route("api/pagamentos")]
[Authorize]
public class PaymentsController(
    PrintFlowDbContext db,
    SecurityService securityService,
    MercadoPagoPaymentService mercadoPagoPaymentService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("mercado-pago/config")]
    [EnableRateLimiting("public-read")]
    public ActionResult<PaymentPublicConfigResponse> MercadoPagoConfig()
    {
        return Ok(new PaymentPublicConfigResponse(mercadoPagoPaymentService.PublicKey, mercadoPagoPaymentService.IsEnabled));
    }

    [HttpPost("{orderId:guid}/pix")]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<PixPaymentResponse>> CreatePix(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetUserOrderAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Pedido nao encontrado." });
        }

        if (order.PaymentMethod != PaymentMethod.Pix)
        {
            return BadRequest(new { message = "Este pedido nao foi criado com pagamento Pix." });
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return BadRequest(new { message = "Este pedido ja esta pago." });
        }

        try
        {
            var payment = await mercadoPagoPaymentService.CreatePixPaymentAsync(order, cancellationToken);
            ApplyMercadoPagoPayment(order, payment);
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new PixPaymentResponse(
                order.Id,
                payment.Status ?? order.PaymentStatus.ToString(),
                payment.Id?.ToString(),
                payment.PointOfInteraction?.TransactionData?.QrCode,
                payment.PointOfInteraction?.TransactionData?.QrCodeBase64,
                payment.PointOfInteraction?.TransactionData?.TicketUrl));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{orderId:guid}/cartao")]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<MercadoPagoPaymentResponse>> PayWithCard(Guid orderId, CardPaymentRequest request, CancellationToken cancellationToken)
    {
        var order = await GetUserOrderAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Pedido nao encontrado." });
        }

        if (order.PaymentMethod != PaymentMethod.Card)
        {
            return BadRequest(new { message = "Este pedido nao foi criado com pagamento por cartao." });
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return BadRequest(new { message = "Este pedido ja esta pago." });
        }

        try
        {
            var payment = await mercadoPagoPaymentService.CreateCardPaymentAsync(order, request, cancellationToken);
            ApplyMercadoPagoPayment(order, payment);
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new MercadoPagoPaymentResponse(
                order.Id,
                payment.Status ?? string.Empty,
                order.PaymentStatus.ToString(),
                payment.Id?.ToString(),
                payment.StatusDetail));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{orderId:guid}/confirmar-manual")]
    [Authorize(Roles = "Admin,Finance")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> ConfirmManualPayment(Guid orderId, ConfirmManualPaymentRequest request, CancellationToken cancellationToken)
    {
        if (!await securityService.ValidateAdminActionPasswordAsync(request.AdminPassword, cancellationToken))
        {
            return Forbid();
        }

        var order = await db.Orders.Include(item => item.Payment).FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Pedido nao encontrado." });
        }

        if (order.PaymentMethod != PaymentMethod.Pickup)
        {
            return BadRequest(new { message = "Confirmacao manual esta disponivel apenas para pagamento no balcao." });
        }

        order.PaymentStatus = PaymentStatus.Paid;
        order.Status = OrderStatus.PaymentConfirmed;
        order.UpdatedAt = DateTime.UtcNow;
        order.Payment ??= new Payment { OrderId = order.Id, Method = order.PaymentMethod, Amount = order.Total };
        order.Payment.Status = PaymentStatus.Paid;
        order.Payment.TransactionId = request.TransactionId;
        order.Payment.ReceiptUrl = request.ReceiptUrl;
        order.Payment.PaidAt = DateTime.UtcNow;
        order.Payment.UpdatedAt = DateTime.UtcNow;
        order.History.Add(new OrderHistory { Status = "Pagamento confirmado manualmente", Notes = "Confirmado pelo financeiro/admin." });

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Pagamento confirmado manualmente." });
    }

    [AllowAnonymous]
    [HttpPost("webhook/mercado-pago")]
    public async Task<IActionResult> MercadoPagoWebhook(CancellationToken cancellationToken)
    {
        var paymentId = await ExtractPaymentIdAsync(cancellationToken);
        if (paymentId is null)
        {
            return Ok();
        }

        var mercadoPagoPayment = await mercadoPagoPaymentService.GetPaymentAsync(paymentId.Value, cancellationToken);
        if (!Guid.TryParse(mercadoPagoPayment.ExternalReference, out var orderId))
        {
            return Ok();
        }

        var order = await db.Orders.Include(item => item.Payment).FirstOrDefaultAsync(item => item.Id == orderId, cancellationToken);
        if (order is null)
        {
            return Ok();
        }

        ApplyMercadoPagoPayment(order, mercadoPagoPayment);
        await db.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    private async Task<Order?> GetUserOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await db.Orders
            .Include(item => item.User)
            .Include(item => item.Product)
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == orderId && item.UserId == User.GetUserId(), cancellationToken);
    }

    private static void ApplyMercadoPagoPayment(Order order, MercadoPago.Resource.Payment.Payment mercadoPagoPayment)
    {
        order.Payment ??= new Payment { OrderId = order.Id, Method = order.PaymentMethod, Amount = order.Total };
        order.Payment.Method = order.PaymentMethod;
        order.Payment.Amount = order.Total;
        order.Payment.Provider = "mercado-pago";
        order.Payment.ProviderReference = mercadoPagoPayment.Id?.ToString();
        order.Payment.TransactionId = mercadoPagoPayment.Id?.ToString();
        order.Payment.ReceiptUrl = mercadoPagoPayment.PointOfInteraction?.TransactionData?.TicketUrl;
        order.Payment.UpdatedAt = DateTime.UtcNow;

        var status = MapPaymentStatus(mercadoPagoPayment.Status);
        order.Payment.Status = status;
        order.PaymentStatus = status;
        if (status == PaymentStatus.Paid)
        {
            order.Status = OrderStatus.PaymentConfirmed;
            order.Payment.PaidAt ??= DateTime.UtcNow;
        }
        else if (status is PaymentStatus.Failed or PaymentStatus.Cancelled or PaymentStatus.Refunded)
        {
            order.Status = status == PaymentStatus.Refunded ? OrderStatus.Cancelled : order.Status;
        }

        order.UpdatedAt = DateTime.UtcNow;
        order.History.Add(new OrderHistory
        {
            Status = $"Mercado Pago: {mercadoPagoPayment.Status}",
            Notes = mercadoPagoPayment.StatusDetail
        });
    }

    private static PaymentStatus MapPaymentStatus(string? status)
    {
        return status switch
        {
            "approved" or "authorized" => PaymentStatus.Paid,
            "rejected" or "cancelled" or "charged_back" => PaymentStatus.Failed,
            "refunded" => PaymentStatus.Refunded,
            _ => PaymentStatus.Pending
        };
    }

    private async Task<long?> ExtractPaymentIdAsync(CancellationToken cancellationToken)
    {
        if (long.TryParse(Request.Query["data.id"], out var queryDataId))
        {
            return queryDataId;
        }

        if (long.TryParse(Request.Query["id"], out var queryId))
        {
            return queryId;
        }

        using var body = await JsonDocument.ParseAsync(Request.Body, cancellationToken: cancellationToken);
        if (body.RootElement.TryGetProperty("data", out var data) &&
            data.TryGetProperty("id", out var dataId) &&
            long.TryParse(dataId.ToString(), out var parsedDataId))
        {
            return parsedDataId;
        }

        if (body.RootElement.TryGetProperty("id", out var id) && long.TryParse(id.ToString(), out var parsedId))
        {
            return parsedId;
        }

        return null;
    }
}
