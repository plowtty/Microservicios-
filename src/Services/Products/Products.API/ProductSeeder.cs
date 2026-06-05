using Products.Domain.Entities;
using Products.Domain.ValueObjects;
using Products.Infrastructure.Persistence;

public static class ProductSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ProductsDbContext>();

        if (db.Products.Any()) return;

        var products = new[]
        {
            Product.Create("Laptop Pro 15",     "High-performance 15-inch laptop",          "Electronics", new Money(1299.99m, "USD"), 50),
            Product.Create("Wireless Mouse",    "Ergonomic wireless mouse with USB receiver","Electronics", new Money(29.99m,  "USD"), 200),
            Product.Create("Mechanical Keyboard","Tactile mechanical keyboard, RGB backlit", "Electronics", new Money(89.99m,  "USD"), 150),
            Product.Create("USB-C Hub 7-in-1",  "Multiport USB-C hub with HDMI and SD card","Electronics", new Money(49.99m,  "USD"), 100),
            Product.Create("Monitor 27 4K",     "27-inch 4K IPS monitor, 144Hz",            "Electronics", new Money(599.99m, "USD"), 30),
            Product.Create("Running Shoes",     "Lightweight breathable running shoes",      "Sports",      new Money(119.99m, "USD"), 80),
            Product.Create("Yoga Mat",          "Non-slip yoga mat, 6mm thick",             "Sports",      new Money(34.99m,  "USD"), 120),
            Product.Create("Water Bottle 1L",   "Insulated stainless steel water bottle",   "Sports",      new Money(24.99m,  "USD"), 300),
            Product.Create("Backpack 30L",      "Waterproof hiking and travel backpack",     "Bags",        new Money(79.99m,  "USD"), 60),
            Product.Create("Desk Lamp LED",     "Adjustable LED desk lamp with USB port",   "Home",        new Money(44.99m,  "USD"), 90),
        };

        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();
    }
}
