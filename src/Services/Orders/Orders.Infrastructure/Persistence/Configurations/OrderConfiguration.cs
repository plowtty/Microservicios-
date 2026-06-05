namespace Orders.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>();
        builder.Property(o => o.CreatedAt).HasColumnName("created_at");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        builder.OwnsOne(o => o.CustomerId, cid =>
        {
            cid.Property(c => c.Value).HasColumnName("customer_id");
        });

        builder.OwnsOne(o => o.ShippingAddress, addr =>
        {
            addr.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(200);
            addr.Property(a => a.City).HasColumnName("address_city").HasMaxLength(100);
            addr.Property(a => a.State).HasColumnName("address_state").HasMaxLength(100);
            addr.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(100);
            addr.Property(a => a.ZipCode).HasColumnName("address_zip_code").HasMaxLength(20);
        });

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(o => o.DomainEvents);
    }
}
