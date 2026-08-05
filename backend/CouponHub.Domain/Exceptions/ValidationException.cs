namespace CouponHub.Domain.Exceptions;

public sealed class ValidationException : DomainException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(
        string message,
        IReadOnlyList<string>? errors = null)
        : base(message)
    {
        Errors = errors ?? [];
    }
}