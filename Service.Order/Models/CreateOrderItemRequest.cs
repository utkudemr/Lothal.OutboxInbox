namespace Service.Order.Models;

public class CreateOrderItemRequest
{
    public decimal Price { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
}