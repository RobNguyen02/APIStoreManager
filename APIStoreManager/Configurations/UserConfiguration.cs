using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using APIStoreManager.Models;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
               .ValueGeneratedOnAdd();

        builder.Property(u => u.Username)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasMany(u => u.Shops)
               .WithOne(s => s.Owner)
               .HasForeignKey(s => s.OwnerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
