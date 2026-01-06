using FluentValidation;
using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Validators;

public sealed class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content cannot be empty")
            .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author name cannot be empty")
            .MaximumLength(100).WithMessage("Author name cannot exceed 100 characters");
    }
}
