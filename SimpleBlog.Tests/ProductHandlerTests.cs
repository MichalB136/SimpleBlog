using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimpleBlog.ApiService.Handlers;
using SimpleBlog.Common;
using SimpleBlog.Common.Api.Configuration;
using SimpleBlog.Common.Models;
using SimpleBlog.Common.Interfaces;
using Xunit;

namespace SimpleBlog.Tests;

public class ProductHandlerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk_WithProducts()
    {
        // Arrange
        var repoMock = new Mock<IProductRepository>();
        var sampleProduct = new Product(
            Guid.NewGuid(),
            "Test product",
            "Desc",
            9.99m,
            null,
            "Books",
            10,
            DateTimeOffset.UtcNow,
            new List<Tag>(),
            new List<string>());

        var paged = new PaginatedResult<Product>
        {
            Items = new List<Product> { sampleProduct },
            Total = 1,
            Page = 1,
            PageSize = 10
        };

        repoMock.Setup(r => r.GetAllAsync(It.IsAny<ProductFilterRequest?>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(paged);

        var handler = new ProductHandler(
            repoMock.Object,
            Mock.Of<IValidator<UpdateProductRequest>>(),
            Mock.Of<SimpleBlog.Common.Logging.IOperationLogger>(),
            NullLogger<ProductHandler>.Instance,
            new EndpointConfiguration(),
            new AuthorizationConfiguration());

        var ctx = new DefaultHttpContext();

        // Act
        var result = await handler.GetAll(ctx);

        // Assert
        var ok = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<PaginatedResult<Product>>>(result);
        var returned = ok.Value;
        Assert.Single(returned.Items);
        Assert.Equal(sampleProduct.Id, returned.Items[0].Id);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenProductExists()
    {
        // Arrange
        var repoMock = new Mock<IProductRepository>();
        var id = Guid.NewGuid();
        var product = new Product(id, "P", "D", 1m, null, "Cat", 1, DateTimeOffset.UtcNow, new List<Tag>(), new List<string>());
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);

        var handler = new ProductHandler(
            repoMock.Object,
            Mock.Of<IValidator<UpdateProductRequest>>(),
            Mock.Of<SimpleBlog.Common.Logging.IOperationLogger>(),
            NullLogger<ProductHandler>.Instance,
            new EndpointConfiguration(),
            new AuthorizationConfiguration());

        var ctx = new DefaultHttpContext();

        // Act
        var result = await handler.GetById(id, ctx);

        // Assert
        var ok = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<Product>>(result);
        var returned = ok.Value;
        Assert.Equal(id, returned.Id);
    }
}
