using CouponHub.Domain.Entities;

namespace CouponHub.Application.Abstractions.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
   
    Task<Coupon?> GetByCodeAsync(
        Guid brandId,
        string couponCode,
        CancellationToken cancellationToken = default);

}