using CouponHub.Domain.Common;
using Microsoft.EntityFrameworkCore;
using CouponHub.Application.Abstractions.Repositories;

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
}