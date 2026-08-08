using CouponHub.Application.Brands.Commands.CreateBrand;
using FluentValidation;

namespace CouponHub.Application.Validators.Brands;

public sealed class CreateBrandCommandValidator
    : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));

        RuleFor(x => x.Category)
            .IsInEnum();
    }
}