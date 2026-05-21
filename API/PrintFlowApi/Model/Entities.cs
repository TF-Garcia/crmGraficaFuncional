using System.ComponentModel.DataAnnotations;

namespace PrintFlowApi.Model;

public enum UserRole
{
    Client,
    Admin,
    Production,
    Support,
    Finance
}

public enum OrderStatus
{
    QuoteCreated,
    WaitingPayment,
    PaymentConfirmed,
    WaitingArtwork,
    ArtworkReview,
    ArtworkRejected,
    WaitingCustomerApproval,
    ArtworkApproved,
    InProduction,
    Finishing,
    ReadyForPickup,
    OutForDelivery,
    Finished,
    Cancelled
}

public enum PaymentMethod
{
    Pix,
    Card,
    Pickup
}

public enum PaymentStatus
{
    Pending,
    CounterPayment,
    Paid,
    Failed,
    Refunded,
    Cancelled
}

public enum DeliveryMode
{
    Pickup,
    LocalDelivery
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(140)] public string Name { get; set; } = string.Empty;
    [MaxLength(180)] public string Email { get; set; } = string.Empty;
    [MaxLength(30)] public string Phone { get; set; } = string.Empty;
    [MaxLength(30)] public string? Document { get; set; }
    [MaxLength(260)] public string? Address { get; set; }
    [MaxLength(40)] public string? ContactPreference { get; set; }
    [MaxLength(300)] public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Client;
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<Order> Orders { get; set; } = [];
    public List<Quote> Quotes { get; set; } = [];
}

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(90)] public string Slug { get; set; } = string.Empty;
    [MaxLength(140)] public string Name { get; set; } = string.Empty;
    [MaxLength(90)] public string Category { get; set; } = string.Empty;
    [MaxLength(900)] public string Description { get; set; } = string.Empty;
    [MaxLength(900)] public string ImageUrl { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int BaseDeadlineDays { get; set; }
    public bool AllowUpload { get; set; } = true;
    public bool AllowPickup { get; set; } = true;
    public bool AllowDelivery { get; set; } = true;
    public bool AllowPickupPayment { get; set; }
    public bool RequiresAdvancePayment { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<ProductOption> Options { get; set; } = [];
    public List<ProductQuantity> Quantities { get; set; } = [];
}

public class ProductQuantity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
}

public class ProductOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    [MaxLength(40)] public string Type { get; set; } = string.Empty;
    [MaxLength(120)] public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
    public int DeadlineDeltaDays { get; set; }
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(24)] public string Number { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    [MaxLength(120)] public string Size { get; set; } = string.Empty;
    [MaxLength(120)] public string Material { get; set; } = string.Empty;
    [MaxLength(120)] public string PrintMode { get; set; } = string.Empty;
    [MaxLength(120)] public string Finishing { get; set; } = string.Empty;
    [MaxLength(30)] public string Urgency { get; set; } = "normal";
    public DeliveryMode DeliveryMode { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public OrderStatus Status { get; set; } = OrderStatus.QuoteCreated;
    public decimal Subtotal { get; set; }
    public decimal UrgencyFee { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }
    public int EstimatedDays { get; set; }
    public DateTime? Deadline { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
    [MaxLength(120)] public string? Owner { get; set; }
    [MaxLength(30)] public string Priority { get; set; } = "Normal";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderFile> Files { get; set; } = [];
    public List<OrderHistory> History { get; set; } = [];
    public Payment? Payment { get; set; }
}

public enum QuoteStatus
{
    Draft,
    Saved,
    ConvertedToOrder,
    Expired,
    Cancelled
}

public class Quote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(24)] public string Number { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    [MaxLength(120)] public string Size { get; set; } = string.Empty;
    [MaxLength(120)] public string Material { get; set; } = string.Empty;
    [MaxLength(120)] public string PrintMode { get; set; } = string.Empty;
    [MaxLength(120)] public string Finishing { get; set; } = string.Empty;
    [MaxLength(30)] public string Urgency { get; set; } = "normal";
    public DeliveryMode DeliveryMode { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.Saved;
    public decimal Subtotal { get; set; }
    public decimal UrgencyFee { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }
    public int EstimatedDays { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
    public Guid? ConvertedOrderId { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(15);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class OrderFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    [MaxLength(260)] public string FileName { get; set; } = string.Empty;
    [MaxLength(900)] public string? StorageUrl { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class OrderHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    [MaxLength(160)] public string Status { get; set; } = string.Empty;
    [MaxLength(700)] public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(140)] public string Name { get; set; } = string.Empty;
    [MaxLength(90)] public string Category { get; set; } = string.Empty;
    [MaxLength(30)] public string Unit { get; set; } = string.Empty;
    public decimal Available { get; set; }
    public decimal Minimum { get; set; }
    [MaxLength(140)] public string Supplier { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class StockMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    [MaxLength(30)] public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    [MaxLength(300)] public string Reason { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    [MaxLength(40)] public string Provider { get; set; } = "manual";
    [MaxLength(120)] public string? ProviderReference { get; set; }
    [MaxLength(120)] public string? TransactionId { get; set; }
    [MaxLength(900)] public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
}

public class SystemSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(140)] public string CompanyName { get; set; } = "Vera Grafica Digital";
    [MaxLength(180)] public string CompanyEmail { get; set; } = "atendimento@printflowpro.com.br";
    [MaxLength(30)] public string CompanyPhone { get; set; } = "(11) 98888-2026";
    public bool RequireAdminPasswordForSensitiveActions { get; set; }
    [MaxLength(300)] public string? AdminActionPasswordHash { get; set; }
    public bool AutoStockDeductionEnabled { get; set; }
    public OrderStatus StockDeductionTriggerStatus { get; set; } = OrderStatus.InProduction;
    public bool AllowCustomerQuoteEdit { get; set; } = true;
    public bool AllowCustomerOrderEdit { get; set; }
    public bool AllowCustomerOrderCancellation { get; set; } = true;
    public bool AllowCustomerRefundRequest { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class PasswordResetToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    [MaxLength(128)] public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(80)] public string? IpAddress { get; set; }
    [MaxLength(400)] public string? UserAgent { get; set; }
}
