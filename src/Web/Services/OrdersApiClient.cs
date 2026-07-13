namespace Web.Services;

public record OrderSummary(
    int Id,
    string CustomerName,
    string Status,
    DateTime CreatedAt,
    int ItemCount,
    decimal TotalAmount);

public sealed class OrdersApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<OrderSummary>> GetOrdersAsync(CancellationToken ct = default)
    {
        return await http.GetFromJsonAsync<List<OrderSummary>>("/api/orders", ct) ?? [];
    }
}
