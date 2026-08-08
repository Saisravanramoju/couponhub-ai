using CouponHub.Domain.Common;

namespace CouponHub.Application.Abstractions.Repositories;

public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    void Remove(TEntity entity);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}