using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PS.OrderEvents.Domain.Entities;

namespace PS.OrderEvents.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();          

        builder.Property(o => o.Status)
            .HasConversion<string>()        
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Ignore(o => o.DomainEvents);
    }
}