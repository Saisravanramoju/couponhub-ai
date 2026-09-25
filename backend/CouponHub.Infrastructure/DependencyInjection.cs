using CouponHub.Application.Abstractions.Repositories;
using CouponHub.Infrastructure.Persistence;
using CouponHub.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CouponHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration _)
    {
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "DefaultConnection connection string not found.");
            }

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IBrandRepository, BrandRepository>();

        services.AddScoped<ICouponRepository, CouponRepository>();

        return services;
    }
}
