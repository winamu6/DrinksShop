using Drinks.Models.ViewModel;

namespace Drinks.Models
{
    public class PaymentRequest
    {
        public Dictionary<int, int> InsertedCoins { get; set; }
        public List<CartItem> CartItems { get; set; }
    }
}
