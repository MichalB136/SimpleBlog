using FluentValidation;
using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Validators;

public sealed class UpdateAboutMeRequestValidator : AbstractValidator<UpdateAboutMeRequest>
{
    public UpdateAboutMeRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(10000).WithMessage("Content cannot exceed 10000 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048).WithMessage("ImageUrl cannot exceed 2048 characters")
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("ImageUrl must be a valid absolute URL");
    }
}
