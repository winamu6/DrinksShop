using Drinks.Models.ViewModel;
using Drinks.Repository.CartRepository.CartInterfaces;
using System.Text.Json;

namespace Drinks.Services.CartServices
{
    public class CartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public List<CartItem> GetCart() => _cartRepository.GetCart();

        public void SaveCart(List<CartItem> cart) => _cartRepository.SaveCart(cart);

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
