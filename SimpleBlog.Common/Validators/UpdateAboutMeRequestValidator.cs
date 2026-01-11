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
    }
}
