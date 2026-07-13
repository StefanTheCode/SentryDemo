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

        foreach (var order in orders)
        {
            var items = await db.OrderItems
                .Where(i => i.OrderId == order.Id)
                .ToListAsync(ct);

            decimal total = 0;
            foreach (var item in items)
            {
                var price = await pricing.GetPriceAsync(item.ProductName, ct);
                total += (price?.CurrentPrice ?? 0) * item.Quantity;
            }

            result.Add(new OrderSummary(
                order.Id, order.CustomerName, order.Status, order.CreatedAt,
                items.Count, Math.Round(total, 2)));
        }

        return TypedResults.Ok(result);
    }
}