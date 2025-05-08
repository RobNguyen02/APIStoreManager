using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using APIStoreManager.Models;

public class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.ToTable("Shops");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(200);
        builder.Property(s => s.Description)
               .HasColumnType("nvarchar(max)");

        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasMany(s => s.Products)
               .WithOne(p => p.Shop)
               .HasForeignKey(p => p.ShopId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}