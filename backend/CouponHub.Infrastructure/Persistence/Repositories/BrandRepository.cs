using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using CouponHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CouponHub.Infrastructure.Persistence.Repositories;

public sealed class BrandRepository : IBrandRepository
{
    private readonly ApplicationDbContext _context;

    public BrandRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Brand brand,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<Brand>().AddAsync(brand, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Brand?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Brand>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.Id == id,
                cancellationToken);
    }

    public async Task<Brand?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Brand>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.Name == name,
                cancellationToken);
    }

    public async Task<IEnumerable<Brand>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Brand>()
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        name = name.Trim();
        return await _context.Set<Brand>()
    .AnyAsync(
        b => b.Name.ToLower() == name.ToLower(),
        cancellationToken);
    }
}
