using CouponHub.Domain.Enums;
using CouponHub.Domain.Exceptions;
using CouponHub.Domain.ValueObjects;

namespace CouponHub.Domain.Policies;

public static class CouponPolicy
{
    public static void Validate(
        Guid brandId,
        CouponDetails details)
    {
        // We'll move the code here next.
    }
}