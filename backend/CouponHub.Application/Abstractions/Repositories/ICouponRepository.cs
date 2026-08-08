using CouponHub.Domain.Entities;

namespace CouponHub.Application.Abstractions.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Coupon?> GetByCodeAsync(
        Guid brandId,
        string couponCode,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Coupon>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        Guid brandId,
        string couponCode,
        CancellationToken cancellationToken = default);
}