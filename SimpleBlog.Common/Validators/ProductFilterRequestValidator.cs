using FluentValidation;
using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Validators;

public sealed class ProductFilterRequestValidator : AbstractValidator<ProductFilterRequest>
{
    public ProductFilterRequestValidator()
    {
        When(x => x.TagIds is not null, () =>
        {
            RuleFor(x => x.TagIds)
                .Must(ids => ids!.Count <= 20)
                .WithMessage("Nie można filtrować po więcej niż 20 tagach jednocześnie");

            RuleForEach(x => x.TagIds)
                .NotEmpty()
                .WithMessage("ID tagu nie może być pusty");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Category), () =>
        {
            RuleFor(x => x.Category)
                .MaximumLength(50)
                .WithMessage("Kategoria nie może być dłuższa niż 50 znaków");
        });

        When(x => !string.IsNullOrWhiteSpace(x.SearchTerm), () =>
        {
            RuleFor(x => x.SearchTerm)
                .MinimumLength(2)
                .WithMessage("Wyszukiwane hasło musi mieć co najmniej 2 znaki")
                .MaximumLength(100)
                .WithMessage("Wyszukiwane hasło nie może być dłuższe niż 100 znaków");
        });
    }
}
