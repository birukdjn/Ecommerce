namespace Application.DTOs.Cart
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public List<CartItemDto> Items { get; set; } = [];
        public decimal TotalAmount => Items.Sum(x => x.SubTotal);
        public int TotalItems => Items.Sum(x => x.Quantity);
    }
}