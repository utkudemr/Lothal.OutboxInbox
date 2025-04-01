namespace Service.Order.Models;

public class CreateOrderRequest(List<CreateOrderItemRequest> orderItems)
{
    public int CustomerId { get; set; }
    public List<CreateOrderItemRequest> OrderItems { get; set; } = orderItems;
}