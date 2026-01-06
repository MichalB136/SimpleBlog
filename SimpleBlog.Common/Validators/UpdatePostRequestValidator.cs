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

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        // At least one field must be provided for update
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Title) || 
                      !string.IsNullOrEmpty(x.Content) || 
                      !string.IsNullOrEmpty(x.Author) || 
                      !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("At least one field (Title, Content, Author, or ImageUrl) must be provided for update");
    }
}
