using System.Linq.Expressions;

namespace CouponHub.Application.Abstractions.Specifications;

public abstract class BaseSpecification<TEntity>
    : ISpecification<TEntity>
{
    protected BaseSpecification()
    {
    }

    protected BaseSpecification(
        Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<TEntity, bool>>? Criteria { get; }

    public List<Expression<Func<TEntity, object>>> Includes { get; }
        = new();

    public Expression<Func<TEntity, object>>? OrderBy { get; private set; }

    public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }

    public int? Skip { get; private set; }

    public int? Take { get; private set; }

    public bool IsPagingEnabled { get; private set; }

    protected void AddInclude(
        Expression<Func<TEntity, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void ApplyPaging(
        int skip,
        int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected void ApplyOrderBy(
        Expression<Func<TEntity, object>> expression)
    {
        OrderBy = expression;
    }

    protected void ApplyOrderByDescending(
        Expression<Func<TEntity, object>> expression)
    {
        OrderByDescending = expression;
    }
}