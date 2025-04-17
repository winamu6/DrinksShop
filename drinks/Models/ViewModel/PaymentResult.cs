using drinks.Models.Entities;

namespace drinks.Models.ViewModel
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public decimal ChangeAmount { get; set; }
        public Dictionary<int, int>? ChangeCoins { get; set; }
        public Order? Order { get; set; }
    }
}
