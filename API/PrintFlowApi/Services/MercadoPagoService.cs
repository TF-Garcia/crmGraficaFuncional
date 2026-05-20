using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using PrintFlowApi.Model;

namespace PrintFlowApi.Services;

public class MercadoPagoService(IConfiguration configuration, IWebHostEnvironment environment)
{
    public async Task<(string? PreferenceId, string CheckoutUrl)> CreatePreferenceAsync(Order order, CancellationToken cancellationToken)
    {
        var accessToken = configuration["MercadoPago:AccessToken"];
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("Configure MercadoPago:AccessToken para gerar pagamentos reais.");
        }

        if (order.Product is null || order.User is null)
        {
            throw new InvalidOperationException("O pedido precisa estar carregado com cliente e produto.");
        }

        MercadoPagoConfig.AccessToken = accessToken;

        var publicBaseUrl = configuration["MercadoPago:PublicBaseUrl"]?.TrimEnd('/');
        var frontendBaseUrl = configuration["MercadoPago:FrontendBaseUrl"]?.TrimEnd('/');
        var returnBaseUrl = string.IsNullOrWhiteSpace(frontendBaseUrl)
            ? null
            : $"{frontendBaseUrl}/cliente/dashboard?retorno=mercado-pago&pedido={order.Id}";

        var request = new PreferenceRequest
        {
            ExternalReference = order.Id.ToString(),
            NotificationUrl = string.IsNullOrWhiteSpace(publicBaseUrl)
                ? null
                : $"{publicBaseUrl}/api/pagamentos/mercado-pago/webhook",
            BackUrls = string.IsNullOrWhiteSpace(returnBaseUrl)
                ? null
                : new PreferenceBackUrlsRequest
                {
                    Success = $"{returnBaseUrl}&pagamento=sucesso",
                    Pending = $"{returnBaseUrl}&pagamento=pendente",
                    Failure = $"{returnBaseUrl}&pagamento=falha"
                },
            AutoReturn = string.IsNullOrWhiteSpace(frontendBaseUrl) ? null : "approved",
            Items =
            [
                new PreferenceItemRequest
                {
                    Id = order.Product.Id.ToString(),
                    Title = $"{order.Product.Name} - pedido {order.Number}",
                    Description = order.Product.Description,
                    Quantity = 1,
                    CurrencyId = configuration["MercadoPago:CurrencyId"] ?? "BRL",
                    UnitPrice = order.Total
                }
            ]
        };

        var client = new PreferenceClient();
        var preference = await client.CreateAsync(request, cancellationToken: cancellationToken);
        var useSandbox = configuration.GetValue("MercadoPago:UseSandbox", environment.IsDevelopment());
        var checkoutUrl = useSandbox && !string.IsNullOrWhiteSpace(preference.SandboxInitPoint)
            ? preference.SandboxInitPoint
            : preference.InitPoint;

        if (string.IsNullOrWhiteSpace(checkoutUrl))
        {
            throw new InvalidOperationException("O Mercado Pago nao retornou URL de checkout.");
        }

        return (preference.Id, checkoutUrl);
    }

    public async Task<(Guid? OrderId, string? Status)> GetPaymentStatusAsync(long paymentId, CancellationToken cancellationToken)
    {
        var accessToken = configuration["MercadoPago:AccessToken"];
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return (null, null);
        }

        MercadoPagoConfig.AccessToken = accessToken;
        var client = new PaymentClient();
        var payment = await client.GetAsync(paymentId, cancellationToken: cancellationToken);
        var orderId = Guid.TryParse(payment.ExternalReference, out var parsed) ? parsed : (Guid?)null;
        return (orderId, payment.Status);
    }
}
