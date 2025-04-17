namespace drinks.Models.Entities
{
    public class Coin
    {
        public int Id { get; set; }
        public int Denomination { get; set; }
        public int Count { get; set; }
        public bool IsBlocked { get; set; }
    }
}
