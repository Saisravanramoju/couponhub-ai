using Microsoft.Extensions.DependencyInjection;

namespace CouponHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        return services;
    }
}