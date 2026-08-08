using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using CouponHub.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace CouponHub.Infrastructure.Persistence.Repositories;

public sealed class CouponRepository
    : Repository<Coupon>, ICouponRepository
{
    public CouponRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public override async Task<Coupon> AddAsync(
        Coupon coupon,
        CancellationToken cancellationToken = default)
    {
        await Entities.AddAsync(
            coupon,
            cancellationToken);

        await SaveChangesAsync(
            cancellationToken);

        return await GetByIdAsync(
            coupon.Id,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Coupon was saved but could not be retrieved.");
    }

    public async Task<Coupon?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Entities
            .AsNoTracking()
            .Include(c => c.Brand)
            .FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);
    }

    public async Task<Coupon?> GetByCodeAsync(
        Guid brandId,
        string couponCode,
        CancellationToken cancellationToken = default)
    {
        couponCode = couponCode.Trim();

        return await Entities
            .AsNoTracking()
            .Include(c => c.Brand)
            .FirstOrDefaultAsync(
                c => c.BrandId == brandId &&
                     c.CouponCode.ToLower() == couponCode.ToLower(),
                cancellationToken);
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await Entities
            .AsNoTracking()
            .Include(c => c.Brand)
            .OrderBy(c => c.Brand.Name)
            .ThenBy(c => c.CouponCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(
        Guid brandId,
        string couponCode,
        CancellationToken cancellationToken = default)
    {
        couponCode = couponCode.Trim();

        return await Entities
            .AnyAsync(
                c => c.BrandId == brandId &&
                     c.CouponCode.ToLower() == couponCode.ToLower(),
                cancellationToken);
    }
}