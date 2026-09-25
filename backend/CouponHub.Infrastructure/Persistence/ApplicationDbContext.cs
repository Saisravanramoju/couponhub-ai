using CouponHub.Domain.Entities;
using CouponHub.Application.Personalization;
using CouponHub.Infrastructure.Personalization;
using Microsoft.EntityFrameworkCore;

namespace CouponHub.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    private readonly ICurrentUser? _user;
    private Guid? UserId => _user?.Id;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, ICurrentUser? user = null)
        : base(options)
    {
        _user = user;
    }

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<Coupon> Coupons => Set<Coupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        AccountConfiguration.Configure(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
        modelBuilder.Entity<Coupon>().HasQueryFilter(c => c.OwnerId == null || c.OwnerId == UserId);
    }
}