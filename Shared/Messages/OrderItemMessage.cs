namespace Shared.Messages;

public class OrderItemMessage
{
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal Price { get; set; }
}