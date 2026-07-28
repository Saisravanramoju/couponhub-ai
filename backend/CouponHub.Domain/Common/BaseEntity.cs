namespace CouponHub.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; }

    public DateTime UpdatedAt { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();

        var now = DateTime.UtcNow;

        CreatedAt = now;
        UpdatedAt = now;
    }

    protected void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}