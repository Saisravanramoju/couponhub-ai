using CouponHub.Application.Personalization;
using CouponHub.Domain.Entities;
using CouponHub.Domain.Enums;
using CouponHub.Domain.Exceptions;
using CouponHub.Domain.ValueObjects;

namespace CouponHub.Tests;

public sealed class CouponRankingTests
{
    private static Coupon Offer(DiscountType type = DiscountType.Percentage, decimal value = 20,
        decimal? min = 500, decimal? max = 100)
    {
        var brand = new Brand("Example", BrandCategory.Shopping);
        var coupon = new Coupon(brand.Id, new CouponDetails("test20", "Example offer", CouponCategory.Shopping,
            type, value, min, max, DateTime.UtcNow.AddDays(1), CouponSource.Manual));
        typeof(Coupon).GetProperty(nameof(Coupon.Brand))!.SetValue(coupon, brand);
        return coupon;
    }
    [Fact] public void PercentageSavingsRespectCap() => Assert.Equal(100m, CouponRanking.Savings(Offer(), 1000));
    [Fact] public void UnknownBasketDoesNotInventSavings() => Assert.Null(CouponRanking.Savings(Offer(), null));
    [Fact] public void FreeDeliveryDoesNotInventSavings() => Assert.Null(CouponRanking.Savings(Offer(DiscountType.FreeDelivery, 0, 100, null), 500));
    [Fact] public void BelowMinimumIsIneligible() => Assert.False(CouponRanking.Eligible(Offer(), DateTime.UtcNow, 499));
    [Fact] public void ExpiredIsIneligible() => Assert.False(CouponRanking.Eligible(Offer(), DateTime.UtcNow.AddDays(2), 1000));
    [Fact] public void InactiveIsIneligible() { var c = Offer(); c.Deactivate(); Assert.False(CouponRanking.Eligible(c, DateTime.UtcNow, 1000)); }
    [Fact] public void InactiveBrandIsIneligible() { var c = Offer(); c.Brand.Deactivate(); Assert.False(CouponRanking.Eligible(c, DateTime.UtcNow, 1000)); }
    [Fact] public void CodesAreNormalized() => Assert.Equal("TEST20", Offer().CouponCode);
    [Fact] public void InvalidPercentageRejected() => Assert.Throws<DomainException>(() => Offer(value: 101));
    [Fact] public void PercentageAndRupeeCapAreDifferentUnits() => Assert.Equal(5m, CouponRanking.Savings(Offer(value: 20, max: 5), 1000));
    [Fact] public void InvalidEnumRejected() => Assert.Throws<DomainException>(() => Offer((DiscountType)999));
    [Fact] public void PreferencesImproveRanking()
    {
        var c = Offer();
        Assert.True(CouponRanking.Score(c, "", ["Shopping"], new HashSet<Guid>(), null) > CouponRanking.Score(c, "", [], new HashSet<Guid>(), null));
    }
}
