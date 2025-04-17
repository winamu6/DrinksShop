using drinks.Models.ViewModel;
using System.Text.Json;

namespace drinks.Services
{
    public class CartService
    {
        private const string CartSessionKey = "Cart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
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

        public void AddToCart(Product product, int quantity = 1)
        {
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(item => item.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    BrandName = product.Brand?.Name ?? string.Empty,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCart(cart);
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var itemToRemove = cart.FirstOrDefault(item => item.ProductId == productId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCart(cart);
            }
        }

        public void UpdateQuantity(int productId, int newQuantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(item => item.ProductId == productId);

            if (item != null)
            {
                item.Quantity = newQuantity;
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            SaveCart(new List<CartItem>());
        }

        public int GetCartCount()
        {
            return GetCart().Sum(item => item.Quantity);
        }

        public decimal GetCartTotal()
        {
            return GetCart().Sum(item => item.Total);
        }

        public void IncreaseQuantity(int productId, int amount = 1)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity += amount;
                SaveCart(cart);
            }
        }

        public void DecreaseQuantity(int productId, int amount = 1)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity = Math.Max(1, item.Quantity - amount);
                SaveCart(cart);
            }
        }

        public void SetQuantity(int productId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                RemoveFromCart(productId);
                return;
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity = newQuantity;
                SaveCart(cart);
            }
        }
    }
}
