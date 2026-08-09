using CouponHub.Domain.Common;
using Microsoft.EntityFrameworkCore;
using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Application.Abstractions.Specifications;
using CouponHub.Infrastructure.Persistence.Specifications;

namespace CouponHub.Infrastructure.Persistence.Repositories.Base;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context;

    protected DbSet<TEntity> Entities =>
        Context.Set<TEntity>();

    public Repository(ApplicationDbContext context)
    {
        Context = context;
    }

    public virtual async Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await Entities.AddAsync(
            entity,
            cancellationToken);

        await SaveChangesAsync(cancellationToken);

        return entity;
    }

    public virtual void Remove(TEntity entity)
    {
        Entities.Remove(entity);
    }

    public virtual async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await Context.SaveChangesAsync(
            cancellationToken);
    }
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
    ISpecification<TEntity> specification,
    CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator
            .GetQuery(Entities.AsQueryable(), specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(
    ISpecification<TEntity> specification,
    CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator
            .GetQuery(Entities.AsQueryable(), specification)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(
    ISpecification<TEntity> specification,
    CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator
            .GetQuery(Entities.AsQueryable(), specification)
            .AnyAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(
    ISpecification<TEntity> specification,
    CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator
            .GetQuery(Entities.AsQueryable(), specification)
            .CountAsync(cancellationToken);
    }
}