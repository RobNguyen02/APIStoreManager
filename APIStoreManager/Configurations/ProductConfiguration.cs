using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using APIStoreManager.Models;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.HasIndex(p => p.Name)
               .IsUnique();

        builder.Property(p => p.Description)
               .HasColumnType("nvarchar(max)");


        builder.Property(p => p.Price)
               .HasColumnType("double")
               .IsRequired();

        builder.HasIndex(p => p.Price);

        builder.HasOne(p => p.Shop)
               .WithMany(s => s.Products)
               .HasForeignKey(p => p.ShopId)
               .OnDelete(DeleteBehavior.Cascade);
    }

}
