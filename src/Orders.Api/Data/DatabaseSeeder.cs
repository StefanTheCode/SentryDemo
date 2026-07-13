using Microsoft.EntityFrameworkCore;
using Orders.Api.Domain;

namespace Orders.Api.Data;

public static class DatabaseSeeder
{
    private static readonly string[] CustomerNames =
    [
        "Alice Johnson", "Bob Martinez", "Carol Davis", "David Wilson",
        "Emma Thompson", "Frank Garcia", "Grace Lee", "Henry Brown",
        "Isabella White", "James Taylor", "Karen Anderson", "Liam Thomas",
        "Maria Jackson", "Noah Harris", "Olivia Clark", "Peter Lewis",
    ];

    private static readonly string[] ProductNames =
    [
        "Wireless Headphones", "Mechanical Keyboard", "USB-C Hub", "Monitor Stand",
        "Webcam HD 4K", "Laptop Stand Aluminum", "XL Mouse Pad", "Cable Management Kit",
        "LED Desk Lamp", "Ergonomic Chair Cushion", "Bluetooth Speaker", "Phone Holder",
        "Surge Protector 6-port", "Screen Cleaner Kit", "Desk Organizer", "Trackpad Pro",
        "Wrist Rest Gel", "Privacy Screen Filter", "Portable SSD 1TB", "Docking Station",
    ];

    private static readonly string[] Statuses =
        ["Pending", "Processing", "Shipped", "Delivered"];

    public static async Task SeedAsync(OrdersDbContext db, CancellationToken ct = default)
    {
        await db.Database.EnsureCreatedAsync(ct);

        if (await db.Orders.AnyAsync(ct))
            return;

        var random = new Random(42);
        var orders = new List<Order>();

        // 40 orders × 4–8 items  ≈  240 items.
        // Enough to make the distributed N+1 catastrophic without bloating the DB.
        for (int i = 0; i < 40; i++)
        {
            var order = new Order
            {
                CustomerName = CustomerNames[random.Next(CustomerNames.Length)],
                Status       = Statuses[random.Next(Statuses.Length)],
                CreatedAt    = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                Items        = []
            };

            int itemCount = random.Next(4, 9);
            for (int j = 0; j < itemCount; j++)
            {
                order.Items.Add(new OrderItem
                {
                    ProductName = ProductNames[random.Next(ProductNames.Length)],
                    Quantity    = random.Next(1, 5),
                });
            }

            orders.Add(order);
        }

        db.Orders.AddRange(orders);
        await db.SaveChangesAsync(ct);
    }
}

file static class DbContextExtensions
{
    public static Task<bool> AnyAsync<T>(this Microsoft.EntityFrameworkCore.DbSet<T> set, CancellationToken ct) where T : class
        => Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(set, ct);
}
