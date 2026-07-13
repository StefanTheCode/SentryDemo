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

        var result = new List<OrderSummary>(orders.Count);

        var allItems = await db.OrderItems
            .Where(i => orders.Select(o => o.Id).Contains(i.OrderId))
            .ToListAsync(ct);

        var allProductNames = allItems.Select(i => i.ProductName).Distinct();
        var priceList = await pricing.GetPricesBatchAsync(allProductNames, ct);
        var priceMap = priceList.ToDictionary(p => p.ProductName, p => p.CurrentPrice, StringComparer.OrdinalIgnoreCase);

        foreach (var order in orders)
        {
            var items = allItems.Where(i => i.OrderId == order.Id).ToList();

            decimal total = 0;
            foreach (var item in items)
            {
                var unitPrice = priceMap.TryGetValue(item.ProductName, out var p) ? p : 0m;
                total += unitPrice * item.Quantity;
            }

            result.Add(new OrderSummary(
                order.Id, order.CustomerName, order.Status, order.CreatedAt,
                items.Count, Math.Round(total, 2)));
        }

        return TypedResults.Ok(result);
    }
}