using CouponHub.Application.Abstractions.Specifications;

public interface IRepository<TEntity>
{
    Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    void Remove(TEntity entity);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    // NEW

    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);
}