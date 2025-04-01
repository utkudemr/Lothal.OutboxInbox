using Shared.Messages;

namespace Shared.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public int BuyerId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItemMessage> OrderItems { get; set; }
    public Guid IdempotentToken { get; set; }
}
