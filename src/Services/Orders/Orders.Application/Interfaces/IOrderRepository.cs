namespace Orders.Application.Interfaces;

using Orders.Domain.Entities;
using Shared.Common.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
