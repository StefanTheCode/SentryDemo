namespace Orders.Api.Pricing;

public record PriceResponse(string ProductName, decimal CurrentPrice, int DiscountPercent);

public interface IPricingClient
{
    Task<PriceResponse?> GetPriceAsync(string productName, CancellationToken ct);
    Task<IReadOnlyList<PriceResponse>> GetPricesBatchAsync(IEnumerable<string> productNames, CancellationToken ct);
}

public sealed class PricingClient(HttpClient http) : IPricingClient
{
    public async Task<PriceResponse?> GetPriceAsync(string productName, CancellationToken ct)
    {
        var encoded = Uri.EscapeDataString(productName);
        return await http.GetFromJsonAsync<PriceResponse>($"/api/prices/{encoded}", ct);
    }

    public async Task<IReadOnlyList<PriceResponse>> GetPricesBatchAsync(
        IEnumerable<string> productNames, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync(
            "/api/prices/batch",
            new { ProductNames = productNames.Distinct().ToList() },
            ct);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<PriceResponse>>(cancellationToken: ct)
               ?? [];
    }
}
