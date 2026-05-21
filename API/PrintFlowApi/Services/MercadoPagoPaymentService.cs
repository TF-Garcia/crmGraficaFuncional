using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using PrintFlowApi.DTOs;
using PrintFlowApi.Model;
using MercadoPayment = MercadoPago.Resource.Payment.Payment;

namespace PrintFlowApi.Services;

public class MercadoPagoPaymentService(IConfiguration configuration)
{
    public bool IsEnabled => !string.IsNullOrWhiteSpace(AccessToken);
    public string PublicKey => configuration["MercadoPago:PublicKey"] ?? string.Empty;
    private string AccessToken => configuration["MercadoPago:AccessToken"] ?? string.Empty;
    private string? NotificationUrl => configuration["MercadoPago:NotificationUrl"];

    public async Task<MercadoPayment> CreatePixPaymentAsync(Order order, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        return await CreatePaymentAsync(new PaymentCreateRequest
        {
            TransactionAmount = order.Total,
            Description = $"Pedido #{order.Number} - {order.Product?.Name ?? "Grafica"}",
            PaymentMethodId = "pix",
            ExternalReference = order.Id.ToString(),
            NotificationUrl = NotificationUrl,
            DateOfExpiration = DateTime.UtcNow.AddMinutes(configuration.GetValue("MercadoPago:PixExpirationMinutes", 30)),
            Payer = BuildPayer(order)
        }, cancellationToken);
    }

    public async Task<MercadoPayment> CreateCardPaymentAsync(Order order, CardPaymentRequest request, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        return await CreatePaymentAsync(new PaymentCreateRequest
        {
            TransactionAmount = order.Total,
            Token = request.Token,
            Description = $"Pedido #{order.Number} - {order.Product?.Name ?? "Grafica"}",
            Installments = request.Installments,
            PaymentMethodId = request.PaymentMethodId,
            IssuerId = request.IssuerId?.ToString(),
            ExternalReference = order.Id.ToString(),
            NotificationUrl = NotificationUrl,
            Capture = true,
            Payer = new PaymentPayerRequest
            {
                Email = request.PayerEmail,
                Identification = !string.IsNullOrWhiteSpace(request.IdentificationType) && !string.IsNullOrWhiteSpace(request.IdentificationNumber)
                    ? new IdentificationRequest
                    {
                        Type = request.IdentificationType,
                        Number = OnlyDigits(request.IdentificationNumber)
                    }
                    : null
            }
        }, cancellationToken);
    }

    public async Task<MercadoPayment> GetPaymentAsync(long id, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        MercadoPagoConfig.AccessToken = AccessToken;
        return await new PaymentClient().GetAsync(id, cancellationToken: cancellationToken);
    }

    private async Task<MercadoPayment> CreatePaymentAsync(PaymentCreateRequest request, CancellationToken cancellationToken)
    {
        MercadoPagoConfig.AccessToken = AccessToken;
        return await new PaymentClient().CreateAsync(request, cancellationToken: cancellationToken);
    }

    private PaymentPayerRequest BuildPayer(Order order)
    {
        var user = order.User;
        return new PaymentPayerRequest
        {
            Email = user?.Email,
            FirstName = user?.Name,
            Identification = !string.IsNullOrWhiteSpace(user?.Document)
                ? new IdentificationRequest
                {
                    Type = OnlyDigits(user.Document).Length > 11 ? "CNPJ" : "CPF",
                    Number = OnlyDigits(user.Document)
                }
                : null
        };
    }

    private void EnsureConfigured()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Configure MercadoPago:AccessToken para habilitar pagamentos reais.");
        }
    }

    private static string OnlyDigits(string value) => new(value.Where(char.IsDigit).ToArray());
}
