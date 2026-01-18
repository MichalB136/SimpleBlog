using FluentValidation;
using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Validators;

public sealed class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Post title cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Content)
            .MaximumLength(10000).WithMessage("Post content cannot exceed 10000 characters")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.Author)
            .MaximumLength(100).WithMessage("Author name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Author));

        // At least one field must be provided for update
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Title) || 
                      !string.IsNullOrEmpty(x.Content) || 
                      !string.IsNullOrEmpty(x.Author))
            .WithMessage("At least one field (Title, Content, or Author) must be provided for update");
    }
}
