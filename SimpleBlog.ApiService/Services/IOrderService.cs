using SimpleBlog.Common.Models;

namespace SimpleBlog.ApiService.Services;

public interface IOrderService
{
    Task<Order> CreateAsync(CreateOrderRequest request);
    Task<Order?> GetByIdAsync(Guid id);
    Task<PaginatedResult<Order>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<OrderSummary> GetOrdersSummaryAsync(DateTime? from = null, DateTime? to = null);
    Task<IReadOnlyList<SalesByDay>> GetSalesByDayAsync(DateTime? from = null, DateTime? to = null, int limit = 30);
    Task<IReadOnlyList<StatusCount>> GetOrderStatusCountsAsync(DateTime? from = null, DateTime? to = null);
}
