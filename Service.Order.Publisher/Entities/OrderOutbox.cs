using Shared.Events;

namespace Service.Order.Publisher.Entities;

public class OrderOutbox<T>
{
    public Guid IdempotentToken { get; set; }
    public DateTime OccuredOn { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string Type { get; set; }
    public T Payload { get; set; }
}