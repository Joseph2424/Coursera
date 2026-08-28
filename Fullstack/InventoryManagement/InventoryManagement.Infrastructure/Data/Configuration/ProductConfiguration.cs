using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configuration;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(table =>
            table.HasCheckConstraint("CK_Products_Quantity_NonNegative", "\"Quantity\" >= 0")
        );

        builder.HasIndex(product => new { product.CategoryId, product.CreatedOn });

        builder.Property(product => product.Quantity).IsConcurrencyToken();
    }
}
