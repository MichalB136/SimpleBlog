using FluentValidation;
using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Validators;

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Product description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Product price must be greater than zero")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Product stock cannot be negative")
            .When(x => x.Stock.HasValue);

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Product category cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        // At least one field must be provided for update
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Name) || 
                      !string.IsNullOrEmpty(x.Description) || 
                      x.Price.HasValue || 
                      x.Stock.HasValue || 
                      !string.IsNullOrEmpty(x.Category) || 
                      !string.IsNullOrEmpty(x.ImageUrl) ||
                      (x.Colors != null && x.Colors.Count > 0))
            .WithMessage("At least one field must be provided for update");
    }
}
