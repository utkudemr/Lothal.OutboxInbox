namespace Service.Stock.Entities;

public class OrderInbox<T>
{
    public Guid IdempotentToken { get; set; }
    public bool Processed { get; set; }
    public T Payload { get; set; }
}