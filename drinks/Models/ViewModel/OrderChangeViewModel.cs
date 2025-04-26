namespace Drinks.Models.ViewModel
{
    public class OrderChangeViewModel
    {
        public decimal ChangeAmount { get; set; }
        public List<Coin> ChangeCoins { get; set; } = new List<Coin>();

        public class Coin
        {
            public int Denomination { get; set; }
            public int Count { get; set; }
        }
    }
}
