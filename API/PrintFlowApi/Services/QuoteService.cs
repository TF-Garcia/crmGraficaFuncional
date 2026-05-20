using PrintFlowApi.DTOs;
using PrintFlowApi.Model;

namespace PrintFlowApi.Services;

public class QuoteService(IConfiguration configuration)
{
    private static readonly Dictionary<string, (decimal Multiplier, int Days)> Urgency = new(StringComparer.OrdinalIgnoreCase)
    {
        ["normal"] = (1m, 0),
        ["expressa"] = (1.25m, -1),
        ["urgente"] = (1.45m, -2)
    };

    public QuoteResponse Calculate(Product product, QuoteRequest request)
    {
        var size = FindOption(product, "size", request.Size);
        var material = FindOption(product, "material", request.Material);
        var printMode = FindOption(product, "printMode", request.PrintMode);
        var finishing = FindOption(product, "finishing", request.Finishing);
        var urgency = Urgency.GetValueOrDefault(request.Urgency, Urgency["normal"]);
        var deliveryFee = request.Delivery == DeliveryMode.LocalDelivery
            ? configuration.GetValue("Business:DeliveryFee", 28m)
            : 0m;

        var quantityFactor = Math.Max(request.Quantity / 100m, 1m);
        var basePrice = product.BasePrice * quantityFactor;
        var variable = size.PriceDelta + material.PriceDelta + printMode.PriceDelta + finishing.PriceDelta;
        var subtotal = Round(basePrice + variable * quantityFactor);
        var urgencyFee = Round(subtotal * (urgency.Multiplier - 1m));
        var total = Round(subtotal + urgencyFee + deliveryFee);
        var estimatedDays = Math.Max(1, product.BaseDeadlineDays + size.DeadlineDeltaDays + material.DeadlineDeltaDays + finishing.DeadlineDeltaDays + urgency.Days);

        return new QuoteResponse(
            subtotal,
            urgencyFee,
            deliveryFee,
            total,
            estimatedDays,
            [
                $"{request.Quantity:N0} unidades",
                size.Name,
                material.Name,
                printMode.Name,
                finishing.Name,
                request.Delivery == DeliveryMode.LocalDelivery ? "Entrega local" : "Retirada no balcao"
            ]);
    }

    private static ProductOption FindOption(Product product, string type, string name)
    {
        return product.Options.FirstOrDefault(option =>
                option.Type.Equals(type, StringComparison.OrdinalIgnoreCase) &&
                option.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Opcao invalida para {type}: {name}");
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
