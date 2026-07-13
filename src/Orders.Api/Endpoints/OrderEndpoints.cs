using Microsoft.EntityFrameworkCore;
using Orders.Api.Contracts;
using Orders.Api.Data;
using Orders.Api.Pricing;

namespace Orders.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/orders").WithTags("Orders");

        orders.MapGet("/", GetOrders);

        return app;
    }

    private static async Task<IResult> GetOrders(
        OrdersDbContext db,
        IPricingClient pricing,
        CancellationToken ct)
    {
        var orders = await db.Orders.ToListAsync(ct);

        // Load all order items for all orders
        var orderIds = orders.Select(o => o.Id).ToList();
        var allItems = await db.OrderItems
            .Where(i => orderIds.Contains(i.OrderId))
            .ToListAsync(ct);

        // Fetch all prices in a single batch request
        var productNames = allItems.Select(i => i.ProductName).Distinct();
        var prices = await pricing.GetPricesBatchAsync(productNames, ct);
        var priceMap = prices.ToDictionary(p => p.ProductName, p => p.CurrentPrice, StringComparer.OrdinalIgnoreCase);

        var itemsByOrder = allItems.GroupBy(i => i.OrderId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<OrderSummary>(orders.Count);
        foreach (var order in orders)
        {
            var items = itemsByOrder.TryGetValue(order.Id, out var orderItems) ? orderItems : [];
            decimal total = items.Sum(item =>
                (priceMap.TryGetValue(item.ProductName, out var p) ? p : 0) * item.Quantity);

            result.Add(new OrderSummary(
                order.Id, order.CustomerName, order.Status, order.CreatedAt,
                items.Count, Math.Round(total, 2)));
        }

        return TypedResults.Ok(result);
    }
}