using CouponHub.Domain.Entities;
using CouponHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CouponHub.Infrastructure.Persistence.Configurations;

public sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("brands");

        builder.HasKey(b => b.Id);

        builder.HasIndex(b => b.Name)
       .IsUnique();

        builder.Property(b => b.Id)
               .HasColumnName("id");

        builder.Property(b => b.Name)
               .HasColumnName("name")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(b => b.Category)
               .HasColumnName("category")
               .HasConversion<string>()
               .IsRequired();

        builder.Property(b => b.IsActive)
               .HasColumnName("is_active")
               .IsRequired();

        builder.Property(b => b.CreatedAt)
               .HasColumnName("created_at");

        builder.Property(b => b.UpdatedAt)
               .HasColumnName("updated_at");

        builder.Property(b => b.LogoUrl)
       .HasColumnName("logo_url")
       .HasMaxLength(500)
       .HasConversion(
            imageUrl => imageUrl.Value,
            value => ImageUrl.Create(value));

        builder.HasOne(c => c.Brand)
       .WithMany(b => b.Coupons)
       .HasForeignKey(c => c.BrandId)
       .OnDelete(DeleteBehavior.Restrict);
    }
}