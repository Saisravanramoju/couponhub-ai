using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Domain.Entities;
using CouponHub.Infrastructure.Persistence;
using CouponHub.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace CouponHub.Infrastructure.Persistence.Repositories;



public sealed class BrandRepository
    : Repository<Brand>, IBrandRepository
{
    public BrandRepository(ApplicationDbContext context)
     : base(context)
    {
    }

    public async Task<Brand?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.Id == id,
                cancellationToken);
    }

    public async Task<Brand?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.Name == name,
                cancellationToken);
    }

    public async Task<IEnumerable<Brand>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await Entities
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        name = name.Trim();
        return await Entities
    .AnyAsync(
        b => b.Name.ToLower() == name.ToLower(),
        cancellationToken);
    }
}
