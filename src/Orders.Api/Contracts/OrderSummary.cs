namespace Orders.Api.Contracts;

public record OrderSummary(
    int Id,
    string CustomerName,
    string Status,
    DateTime CreatedAt,
    int ItemCount,
    decimal TotalAmount);
