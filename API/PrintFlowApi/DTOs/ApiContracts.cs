using System.ComponentModel.DataAnnotations;
using PrintFlowApi.Model;

namespace PrintFlowApi.DTOs;

public record RegisterRequest(
    [param: Required, StringLength(140, MinimumLength = 2)] string Name,
    [param: Required, EmailAddress, StringLength(180)] string Email,
    [param: Required, Phone, StringLength(30)] string Phone,
    [param: StringLength(30)] string? Document,
    [param: StringLength(260)] string? Address,
    [param: Required, StringLength(100, MinimumLength = 8)] string Password);

public record LoginRequest(
    [param: Required, EmailAddress, StringLength(180)] string Email,
    [param: Required, StringLength(100, MinimumLength = 1)] string Password);

public record AuthResponse(Guid UserId, string Name, string Email, string Phone, string Role, string Token, DateTime ExpiresAt);

public record ProductOptionResponse(string Name, decimal Price, int Days);
public record ProductResponse(
    Guid Id,
    string Slug,
    string Name,
    string Category,
    string Description,
    string ImageUrl,
    decimal BasePrice,
    int BaseDeadline,
    bool AllowUpload,
    bool AllowPickupPayment,
    bool Active,
    IReadOnlyList<int> Quantities,
    IReadOnlyList<ProductOptionResponse> Sizes,
    IReadOnlyList<ProductOptionResponse> Materials,
    IReadOnlyList<ProductOptionResponse> PrintModes,
    IReadOnlyList<ProductOptionResponse> Finishings);

public record QuoteRequest(
    Guid ProductId,
    [param: Range(1, 100000)] int Quantity,
    [param: Required, StringLength(120)] string Size,
    [param: Required, StringLength(120)] string Material,
    [param: Required, StringLength(120)] string PrintMode,
    [param: Required, StringLength(120)] string Finishing,
    [param: Required, StringLength(30)] string Urgency,
    DeliveryMode Delivery);

public record QuoteResponse(decimal Subtotal, decimal UrgencyFee, decimal DeliveryFee, decimal Total, int EstimatedDays, IReadOnlyList<string> Details);

public record CreateOrderRequest(
    Guid ProductId,
    [param: Range(1, 100000)] int Quantity,
    [param: Required, StringLength(120)] string Size,
    [param: Required, StringLength(120)] string Material,
    [param: Required, StringLength(120)] string PrintMode,
    [param: Required, StringLength(120)] string Finishing,
    [param: Required, StringLength(30)] string Urgency,
    DeliveryMode Delivery,
    PaymentMethod PaymentMethod,
    [param: StringLength(1000)] string? Notes,
    [param: StringLength(260)] string? ArtworkFileName);

public record OrderResponse(
    Guid Id,
    string Number,
    string CustomerName,
    string ProductName,
    int Quantity,
    string Status,
    string PaymentStatus,
    string PaymentMethod,
    decimal Total,
    DateTime? Deadline,
    string? PaymentUrl,
    DateTime CreatedAt);

public record InventoryItemResponse(Guid Id, string Name, string Category, decimal Available, string Unit, decimal Minimum, string Supplier, decimal UnitCost);
public record PaymentPreferenceRequest(Guid OrderId);
public record PaymentPreferenceResponse(Guid OrderId, string? PreferenceId, string CheckoutUrl, string Status);
public record MercadoPagoReturnRequest(Guid OrderId, [param: StringLength(40)] string? PaymentId, [param: StringLength(40)] string? Status);
public record MercadoPagoReturnResponse(Guid OrderId, string OrderStatus, string PaymentStatus, string Message);
