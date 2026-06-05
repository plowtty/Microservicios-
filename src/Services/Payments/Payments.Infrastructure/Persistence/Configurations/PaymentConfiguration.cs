namespace Payments.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.OrderId).HasColumnName("order_id");
        builder.Property(p => p.CustomerId).HasColumnName("customer_id");
        builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>();
        builder.Property(p => p.FailureReason).HasColumnName("failure_reason").HasMaxLength(500);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.ProcessedAt).HasColumnName("processed_at");

        builder.OwnsOne(p => p.Amount, a =>
        {
            a.Property(x => x.Value).HasColumnName("amount");
            a.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        });

        builder.Ignore(p => p.DomainEvents);
    }
}
