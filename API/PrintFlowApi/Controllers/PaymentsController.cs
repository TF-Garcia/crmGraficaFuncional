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
[Route("api/pagamentos")]
[Authorize]
public class PaymentsController(PrintFlowDbContext db, SecurityService securityService) : ControllerBase
{
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
}
