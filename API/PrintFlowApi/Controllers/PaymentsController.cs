using System.Text.Json;
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
[Route("api/pagamentos/mercado-pago")]
public class PaymentsController(PrintFlowDbContext db, MercadoPagoService mercadoPagoService) : ControllerBase
{
    [Authorize]
    [HttpPost("preferencia")]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<PaymentPreferenceResponse>> CreatePreference(PaymentPreferenceRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var order = await db.Orders
            .Include(item => item.User)
            .Include(item => item.Product)
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == request.OrderId && item.UserId == userId, cancellationToken);

        if (order is null)
        {
            return NotFound(new { message = "Pedido nao encontrado." });
        }

        if (order.PaymentMethod == PaymentMethod.Pickup)
        {
            return BadRequest(new { message = "Pedido configurado para pagamento na retirada." });
        }

        try
        {
            var preference = await mercadoPagoService.CreatePreferenceAsync(order, cancellationToken);
            order.PaymentStatus = PaymentStatus.WaitingProvider;
            order.Status = OrderStatus.WaitingPayment;
            order.Payment ??= new Payment { OrderId = order.Id, Method = order.PaymentMethod, Amount = order.Total };
            order.Payment.Provider = "mercado-pago";
            order.Payment.ProviderReference = preference.PreferenceId;
            order.Payment.CheckoutUrl = preference.CheckoutUrl;
            order.Payment.Status = PaymentStatus.WaitingProvider;
            order.History.Add(new OrderHistory { Status = "Checkout gerado", Notes = "Preferencia Mercado Pago criada." });
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new PaymentPreferenceResponse(order.Id, preference.PreferenceId, preference.CheckoutUrl, order.PaymentStatus.ToString()));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch
        {
            return BadRequest(new { message = "Nao foi possivel gerar o checkout do Mercado Pago." });
        }
    }

    [Authorize]
    [HttpPost("retorno")]
    [EnableRateLimiting("write")]
    public async Task<ActionResult<MercadoPagoReturnResponse>> ProcessReturn(MercadoPagoReturnRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var order = await db.Orders
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == request.OrderId && item.UserId == userId, cancellationToken);

        if (order is null)
        {
            return NotFound(new { message = "Pedido nao encontrado." });
        }

        var verifiedStatus = await TryVerifyPaymentStatusAsync(request.PaymentId, request.OrderId, cancellationToken);
        if (verifiedStatus is null)
        {
            return BadRequest(new { message = "Nao foi possivel confirmar o pagamento no Mercado Pago." });
        }

        ApplyPaymentStatus(order, verifiedStatus, request.PaymentId);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new MercadoPagoReturnResponse(
            order.Id,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.PaymentStatus == PaymentStatus.Paid
                ? "Pagamento confirmado. O pedido avancou para envio/conferencia de arte."
                : "Retorno recebido. O status do pagamento foi atualizado."));
    }

    [HttpPost("webhook")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        var paymentId = await TryGetPaymentIdFromRequestAsync(cancellationToken);
        if (!paymentId.HasValue)
        {
            return Ok();
        }

        var providerPayment = await mercadoPagoService.GetPaymentStatusAsync(paymentId.Value, cancellationToken);
        if (providerPayment.OrderId is null || string.IsNullOrWhiteSpace(providerPayment.Status))
        {
            return Ok();
        }

        var order = await db.Orders
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == providerPayment.OrderId.Value, cancellationToken);

        if (order is null)
        {
            return Ok();
        }

        ApplyPaymentStatus(order, providerPayment.Status, paymentId.Value.ToString());
        await db.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    private async Task<string?> TryVerifyPaymentStatusAsync(string? paymentId, Guid orderId, CancellationToken cancellationToken)
    {
        if (!long.TryParse(paymentId, out var parsedPaymentId))
        {
            return null;
        }

        var payment = await mercadoPagoService.GetPaymentStatusAsync(parsedPaymentId, cancellationToken);
        return payment.OrderId == orderId ? payment.Status?.ToLowerInvariant() : null;
    }

    private async Task<long?> TryGetPaymentIdFromRequestAsync(CancellationToken cancellationToken)
    {
        var queryId = Request.Query["data.id"].FirstOrDefault() ?? Request.Query["id"].FirstOrDefault();
        if (long.TryParse(queryId, out var parsedQueryId))
        {
            return parsedQueryId;
        }

        if (Request.ContentLength is null or 0)
        {
            return null;
        }

        using var document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: cancellationToken);
        return document.RootElement.TryGetProperty("data", out var data) &&
               data.TryGetProperty("id", out var idProperty) &&
               long.TryParse(idProperty.GetString(), out var parsedBodyId)
            ? parsedBodyId
            : null;
    }

    private static void ApplyPaymentStatus(Order order, string status, string? paymentId)
    {
        switch (status.ToLowerInvariant())
        {
            case "approved":
                order.Status = OrderStatus.PaymentConfirmed;
                order.PaymentStatus = PaymentStatus.Paid;
                order.Payment!.Status = PaymentStatus.Paid;
                order.Payment.PaidAt = DateTime.UtcNow;
                order.Payment.MercadoPagoPaymentId = paymentId;
                order.History.Add(new OrderHistory { Status = "Pagamento confirmado", Notes = "Confirmado via Mercado Pago." });
                break;
            case "pending":
            case "in_process":
                order.PaymentStatus = PaymentStatus.WaitingProvider;
                order.Payment!.Status = PaymentStatus.WaitingProvider;
                break;
            case "refunded":
                order.PaymentStatus = PaymentStatus.Refunded;
                order.Payment!.Status = PaymentStatus.Refunded;
                break;
            case "cancelled":
            case "rejected":
            case "failed":
            case "failure":
                order.PaymentStatus = PaymentStatus.Failed;
                order.Payment!.Status = PaymentStatus.Failed;
                break;
        }
    }
}
