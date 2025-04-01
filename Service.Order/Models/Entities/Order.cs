namespace Service.Order.Models.Entities;

public class Order
{
    public Guid Id { get; set; }
    public int BuyerId { get; set; }
    public DateTime CreatedDate { get; set; }
    public decimal TotalPrice { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal Price { get; set; }
}