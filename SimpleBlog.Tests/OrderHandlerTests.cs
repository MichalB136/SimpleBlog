using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimpleBlog.ApiService.Handlers;
using SimpleBlog.Common;
using SimpleBlog.Common.Models;
using SimpleBlog.Common.Interfaces;
using SimpleBlog.Common.Logging;
using SimpleBlog.Common.Api.Configuration;
using Xunit;

namespace SimpleBlog.Tests;

public class OrderHandlerTests
{
    [Fact]
    public async Task Create_ReturnsCreated_WhenValid()
    {
        var svcMock = new Mock<SimpleBlog.ApiService.Services.IOrderService>();
        var id = Guid.NewGuid();
        var order = new Order(id, "Test User", "test@example.com", "123", "Addr", "City", "12345", 100m, DateTimeOffset.UtcNow, new List<OrderItem>(), "New");

        svcMock.Setup(r => r.CreateAsync(It.IsAny<CreateOrderRequest>())).ReturnsAsync(order);

        var validatorMock = new Mock<IValidator<CreateOrderRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new OrderHandler(
            svcMock.Object,
            validatorMock.Object,
            Mock.Of<IOperationLogger>(),
            NullLogger<OrderHandler>.Instance,
            new EndpointConfiguration(),
            new AuthorizationConfiguration() );

        var req = new CreateOrderRequest("Test User","test@example.com","123","Addr","City","12345", new List<OrderItemRequest>());

        var result = await handler.Create(req);

        var created = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Created<Order>>(result);
        Assert.Equal(id, created.Value.Id);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenOrderExists()
    {
        var svcMock = new Mock<SimpleBlog.ApiService.Services.IOrderService>();
        var id = Guid.NewGuid();
        var order = new Order(id, "Test User", "test@example.com", "123", "Addr", "City", "12345", 50m, DateTimeOffset.UtcNow, new List<OrderItem>(), "New");
        svcMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(order);

        var handler = new OrderHandler(
            svcMock.Object,
            Mock.Of<IValidator<CreateOrderRequest>>(),
            Mock.Of<IOperationLogger>(),
            NullLogger<OrderHandler>.Instance,
            new EndpointConfiguration(),
            new AuthorizationConfiguration { RequireAdminForOrderView = true });

        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }));

        var result = await handler.GetById(id, ctx);

        var ok = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<Order>>(result);
        Assert.Equal(id, ok.Value.Id);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_ForAdmin()
    {
        var svcMock = new Mock<SimpleBlog.ApiService.Services.IOrderService>();

        var sampleOrder = new Order(Guid.NewGuid(), "User", "u@example.com", "123", "Addr", "City", "00000", 0m, DateTimeOffset.UtcNow, new List<OrderItem>(), "New");
        var paged = new PaginatedResult<Order>
        {
            Items = new List<Order> { sampleOrder },
            Total = 1,
            Page = 1,
            PageSize = 10
        };

        svcMock.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(paged);

        var handler = new OrderHandler(
            svcMock.Object,
            Mock.Of<IValidator<CreateOrderRequest>>(),
            Mock.Of<IOperationLogger>(),
            NullLogger<OrderHandler>.Instance,
            new EndpointConfiguration(),
            new AuthorizationConfiguration { RequireAdminForOrderView = true });

        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }));

        var result = await handler.GetAll(ctx);

        var ok = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<PaginatedResult<Order>>>(result);
        Assert.Single(ok.Value.Items);
    }
}
