using System.ComponentModel.DataAnnotations;

namespace drinks.Models.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
