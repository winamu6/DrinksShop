using drinks.Models.ViewModel;
using Drinks.Repository.CartRepository.CartInterfaces;
using System.Text.Json;

namespace Drinks.Repository.CartRepository
{
    public class SessionCartRepository : ICartRepository
    {
        private const string CartSessionKey = "Cart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionCartRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<CartItem> GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var cartJson = session?.GetString(CartSessionKey);
            return cartJson == null
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;
        }

        public void SaveCart(List<CartItem> cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }

}
