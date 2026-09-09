using CouponHub.Domain.Entities;


namespace CouponHub.Application.Abstractions.Repositories;

public interface IBrandRepository : IRepository<Brand>
{
  
    Task<Brand?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Brand>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}