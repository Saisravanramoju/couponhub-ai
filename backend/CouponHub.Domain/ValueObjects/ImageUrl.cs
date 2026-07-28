using CouponHub.Domain.Exceptions;

namespace CouponHub.Domain.ValueObjects;

public sealed class ImageUrl
{
    public string Value { get; }

    private ImageUrl(string value)
    {
        Value = value;
    }

    public static ImageUrl Create(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Logo URL is required.");

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            throw new DomainException("Invalid logo URL.");

        return new ImageUrl(url.Trim());
    }

    public override string ToString() => Value;
}