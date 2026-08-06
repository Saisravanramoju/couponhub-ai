using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CouponHub.Infrastructure.Persistence.Repositories;

public sealed class CouponRepository : ICouponRepository
{
    private readonly ApplicationDbContext _context;

    public CouponRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Coupon> AddAsync(
    Coupon coupon,
    CancellationToken cancellationToken = default)
    {
        await _context.Set<Coupon>()
            .AddAsync(coupon, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

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
        return await _context.Set<Coupon>()
            .AsNoTracking()
            .Include(c => c.Brand)
            .FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Coupon>()
            .AsNoTracking()
            .Include(c => c.Brand)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(
    Guid brandId,
    string couponCode,
    CancellationToken cancellationToken = default)
    {
        couponCode = couponCode.Trim();

        return await _context.Set<Coupon>()
            .AnyAsync(
                c => c.BrandId == brandId &&
                     c.CouponCode.ToUpper() == couponCode.ToUpper(),
                cancellationToken);
    }
}