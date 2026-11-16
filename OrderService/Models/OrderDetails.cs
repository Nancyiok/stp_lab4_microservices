namespace OrderService.Models
{
    public class OrderDetails
    {
        public Order Order { get; set; } = new Order();
        public User? User { get; set; }
    }
}
