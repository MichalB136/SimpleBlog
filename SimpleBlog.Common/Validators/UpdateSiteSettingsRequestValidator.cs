using FluentValidation;
using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Validators;

public sealed class UpdateSiteSettingsRequestValidator : AbstractValidator<UpdateSiteSettingsRequest>
{
    public UpdateSiteSettingsRequestValidator()
    {
        RuleFor(x => x.Theme)
            .NotEmpty()
            .WithMessage("Theme is required")
            .Must(theme => ThemeNames.All.Contains(theme))
            .WithMessage($"Theme must be one of: {string.Join(", ", ThemeNames.All)}");

        RuleFor(x => x.ContactText)
            .MaximumLength(5000)
            .WithMessage("ContactText cannot exceed 5000 characters");
    }
}
