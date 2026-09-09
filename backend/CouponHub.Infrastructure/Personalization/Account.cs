namespace CouponHub.Infrastructure.Personalization;

public sealed class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool IsAdmin { get; set; }
    public string[] Categories { get; set; } = [];
}
public sealed class Session
{
    public string TokenHash { get; set; } = "";
    public Guid AccountId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
public sealed class SavedCoupon
{
    public Guid AccountId { get; set; }
    public Guid CouponId { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
public sealed class CouponEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid CouponId { get; set; }
    public string Kind { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
