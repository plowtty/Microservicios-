namespace Orders.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context) => _context = context;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Orders
            .Include("_items")
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        await _context.Orders
            .Include("_items")
            .Where(o => o.CustomerId.Value == customerId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        entity.ClearDomainEvents();
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        entity.ClearDomainEvents();
    }

    public async Task DeleteAsync(Order entity, CancellationToken cancellationToken = default)
    {
        _context.Orders.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
