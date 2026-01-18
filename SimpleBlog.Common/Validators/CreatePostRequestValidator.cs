using FluentValidation;
using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Validators;

public sealed class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Post title cannot be empty")
            .MaximumLength(200).WithMessage("Post title cannot exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Post content cannot be empty")
            .MaximumLength(10000).WithMessage("Post content cannot exceed 10000 characters");

        RuleFor(x => x.Author)
            .MaximumLength(100).WithMessage("Author name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Author));
    }
}
