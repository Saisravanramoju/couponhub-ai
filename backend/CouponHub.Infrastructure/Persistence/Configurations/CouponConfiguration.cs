using CouponHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CouponHub.Infrastructure.Persistence.Configurations;

public sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");

        // Primary Key
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .HasColumnName("id");

        // Foreign Key
        builder.Property(c => c.BrandId)
               .HasColumnName("brand_id")
               .IsRequired();

        builder.HasOne(c => c.Brand)
               .WithMany(b => b.Coupons)
               .HasForeignKey(c => c.BrandId)
               .OnDelete(DeleteBehavior.Restrict);

        // Coupon Code
        builder.Property(c => c.CouponCode)
               .HasColumnName("coupon_code")
               .HasMaxLength(100)
               .IsRequired();
        
        // Description
        builder.Property(c => c.Description)
               .HasColumnName("description")
               .HasMaxLength(500)
               .IsRequired();

        // Enums
        builder.Property(c => c.Category)
               .HasColumnName("category")
               .HasConversion<string>()
               .IsRequired();

        builder.Property(c => c.DiscountType)
               .HasColumnName("discount_type")
               .HasConversion<string>()
               .IsRequired();

        builder.Property(c => c.CouponSource)
               .HasColumnName("coupon_source")
               .HasConversion<string>()
               .IsRequired();

        // Monetary Values
        builder.Property(c => c.DiscountValue)
               .HasColumnName("discount_value")
               .HasPrecision(10, 2)
               .IsRequired();

        builder.Property(c => c.MinimumOrderAmount)
               .HasColumnName("minimum_order_amount")
               .HasPrecision(10, 2);

        builder.Property(c => c.MaximumDiscount)
               .HasColumnName("maximum_discount")
               .HasPrecision(10, 2);

        // Expiry
        builder.Property(c => c.ExpiryDate)
               .HasColumnName("expiry_date");

        // Status
        builder.Property(c => c.IsActive)
               .HasColumnName("is_active")
               .IsRequired();

        // Audit Fields
        builder.Property(c => c.CreatedAt)
               .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
               .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(c => new { c.BrandId, c.CouponCode })
        .IsUnique();

        builder.HasIndex(c => c.IsActive);
    }
}