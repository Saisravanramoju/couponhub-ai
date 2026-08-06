using CouponHub.Application.Brands.Commands.CreateBrand;
using CouponHub.Application.Brands.Queries.GetBrandById;
using CouponHub.Application.Brands.Queries.GetBrands;
using CouponHub.Application.Coupons.Commands.CreateCoupon;
using CouponHub.Application.Coupons.Queries.GetCouponById;
using CouponHub.Application.Coupons.Queries.GetCoupons;
using Microsoft.Extensions.DependencyInjection;

namespace CouponHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateBrandCommandHandler>();
        services.AddScoped<GetBrandByIdQueryHandler>();
        services.AddScoped<GetBrandsQueryHandler>();
        services.AddScoped<CreateCouponCommandHandler>();
        services.AddScoped<GetCouponByIdQueryHandler>();
        services.AddScoped<GetCouponsQueryHandler>();


        return services;
    }
}
