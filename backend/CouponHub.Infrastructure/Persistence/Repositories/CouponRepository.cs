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

    public async Task AddAsync(
        Coupon coupon,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<Coupon>().AddAsync(
            coupon,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
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
}