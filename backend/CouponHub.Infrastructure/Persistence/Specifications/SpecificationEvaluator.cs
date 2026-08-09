using CouponHub.Application.Abstractions.Specifications;
using Microsoft.EntityFrameworkCore;

namespace CouponHub.Infrastructure.Persistence.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<TEntity> GetQuery<TEntity>(
        IQueryable<TEntity> query,
        ISpecification<TEntity> specification)
        where TEntity : class
    {
        // Apply WHERE
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply INCLUDEs
        query = specification.Includes.Aggregate(
            query,
            (current, include) => current.Include(include));

        // Apply ORDER BY
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }

        // Apply ORDER BY DESC
        if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(
                specification.OrderByDescending);
        }

        // Apply Paging
        if (specification.IsPagingEnabled)
        {
            query = query
                .Skip(specification.Skip!.Value)
                .Take(specification.Take!.Value);
        }

        return query;
    }
}