using CouponHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CouponHub.Infrastructure.Personalization;

public static class AccountConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        model.Entity<Account>(b => {
            b.ToTable("accounts"); b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasMaxLength(254); b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.PasswordHash).HasMaxLength(500);
        });
        model.Entity<Session>(b => {
            b.ToTable("sessions"); b.HasKey(x => x.TokenHash);
            b.Property(x => x.TokenHash).HasMaxLength(64);
            b.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.ExpiresAt);
        });
        model.Entity<SavedCoupon>(b => {
            b.ToTable("saved_coupons"); b.HasKey(x => new { x.AccountId, x.CouponId });
            b.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Coupon>().WithMany().HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<CouponEvent>(b => {
            b.ToTable("coupon_events"); b.HasKey(x => x.Id);
            b.Property(x => x.Kind).HasMaxLength(20);
            b.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Coupon>().WithMany().HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.AccountId, x.CreatedAt });
        });
    }
}
