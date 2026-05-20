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

public record ForgotPasswordRequest([param: Required, EmailAddress, StringLength(180)] string Email);

public record ResetPasswordRequest(
    [param: Required, StringLength(220, MinimumLength = 20)] string Token,
    [param: Required, StringLength(100, MinimumLength = 8)] string Password);

public record ProfileResponse(Guid Id, string Name, string Email, string Phone, string? Document, string? Address, string? ContactPreference, bool Active, DateTime CreatedAt);

public record UpdateProfileRequest(
    [param: Required, StringLength(140, MinimumLength = 2)] string Name,
    [param: Required, Phone, StringLength(30)] string Phone,
    [param: StringLength(30)] string? Document,
    [param: StringLength(260)] string? Address,
    [param: StringLength(40)] string? ContactPreference,
    [param: StringLength(100, MinimumLength = 8)] string? Password);

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
    bool AllowPickup,
    bool AllowDelivery,
    bool AllowPickupPayment,
    bool RequiresAdvancePayment,
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

public record CreateQuoteRequest(
    Guid ProductId,
    [param: Range(1, 100000)] int Quantity,
    [param: Required, StringLength(120)] string Size,
    [param: Required, StringLength(120)] string Material,
    [param: Required, StringLength(120)] string PrintMode,
    [param: Required, StringLength(120)] string Finishing,
    [param: Required, StringLength(30)] string Urgency,
    DeliveryMode Delivery,
    [param: StringLength(1000)] string? Notes,
    bool Draft = false);

public record QuoteSavedResponse(
    Guid Id,
    string Number,
    string ProductName,
    int Quantity,
    string Status,
    decimal Total,
    int EstimatedDays,
    DateTime ExpiresAt,
    DateTime CreatedAt);

public record ConvertQuoteRequest(PaymentMethod PaymentMethod, [param: StringLength(260)] string? ArtworkFileName);

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
    DateTime CreatedAt);

public record UpdateOrderStatusRequest(OrderStatus Status, [param: StringLength(1000)] string? InternalNotes, string? AdminPassword);
public record ConfirmManualPaymentRequest(string? TransactionId, string? ReceiptUrl, string? AdminPassword);

public record InventoryItemResponse(Guid Id, string Name, string Category, decimal Available, string Unit, decimal Minimum, string Supplier, decimal UnitCost, bool Active);
public record StockMovementRequest(Guid InventoryItemId, [param: Required, StringLength(30)] string Type, [param: Range(0.01, 1000000)] decimal Quantity, [param: Required, StringLength(300)] string Reason, Guid? OrderId, string? AdminPassword);

public record SystemSettingsResponse(
    Guid Id,
    string CompanyName,
    string CompanyEmail,
    string CompanyPhone,
    bool RequireAdminPasswordForSensitiveActions,
    bool HasAdminActionPassword,
    bool AutoStockDeductionEnabled,
    string StockDeductionTriggerStatus);

public record UpdateSystemSettingsRequest(
    [param: Required, StringLength(140)] string CompanyName,
    [param: Required, EmailAddress, StringLength(180)] string CompanyEmail,
    [param: Required, Phone, StringLength(30)] string CompanyPhone,
    bool RequireAdminPasswordForSensitiveActions,
    [param: StringLength(100, MinimumLength = 8)] string? AdminActionPassword,
    bool AutoStockDeductionEnabled,
    OrderStatus StockDeductionTriggerStatus,
    string? CurrentAdminPassword);
