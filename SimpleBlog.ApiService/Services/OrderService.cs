using SimpleBlog.Common.Interfaces;
using SimpleBlog.Common.Logging;
using SimpleBlog.Common.Models;

namespace SimpleBlog.ApiService.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IOperationLogger _operationLogger;
    private readonly IEmailService _emailService;

    public OrderService(
        IOrderRepository repository,
        IOperationLogger operationLogger,
        IEmailService emailService)
    {
        _repository = repository;
        _operationLogger = operationLogger;
        _emailService = emailService;
    }

    public async Task<Order> CreateAsync(CreateOrderRequest request)
    {
        var created = await _repository.CreateAsync(request);
        await _emailService.SendOrderConfirmationAsync(request.CustomerEmail, request.CustomerName, created);
        return created;
    }

    public Task<Order?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public Task<PaginatedResult<Order>> GetAllAsync(int page = 1, int pageSize = 10) => _repository.GetAllAsync(page, pageSize);

    public Task<OrderSummary> GetOrdersSummaryAsync(DateTime? from = null, DateTime? to = null) => _repository.GetOrdersSummaryAsync(from, to);

    public Task<IReadOnlyList<SalesByDay>> GetSalesByDayAsync(DateTime? from = null, DateTime? to = null, int limit = 30) => _repository.GetSalesByDayAsync(from, to, limit);

    public Task<IReadOnlyList<StatusCount>> GetOrderStatusCountsAsync(DateTime? from = null, DateTime? to = null) => _repository.GetOrderStatusCountsAsync(from, to);
}
