using FluentValidation;
using SimpleBlog.Common.Models;

namespace SimpleBlog.Common.Validators;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name cannot be empty")
            .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Customer email cannot be empty")
            .EmailAddress().WithMessage("Customer email is invalid")
            .MaximumLength(200).WithMessage("Customer email cannot exceed 200 characters");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("Customer phone cannot be empty")
            .MaximumLength(50).WithMessage("Customer phone cannot exceed 50 characters");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address cannot be empty")
            .MaximumLength(500).WithMessage("Shipping address cannot exceed 500 characters");

        RuleFor(x => x.ShippingCity)
            .NotEmpty().WithMessage("Shipping city cannot be empty")
            .MaximumLength(100).WithMessage("Shipping city cannot exceed 100 characters");

        RuleFor(x => x.ShippingPostalCode)
            .NotEmpty().WithMessage("Shipping postal code cannot be empty")
            .MaximumLength(20).WithMessage("Shipping postal code cannot exceed 20 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(items => items != null && items.Count > 0).WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemRequestValidator());
    }
}

public sealed class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID cannot be empty");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Item quantity must be greater than zero");
    }
}
