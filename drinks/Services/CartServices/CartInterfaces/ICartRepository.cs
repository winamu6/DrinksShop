using drinks.Models.ViewModel;

namespace Drinks.Services.CartServices.CartInterfaces
{
    public interface ICartRepository
    {
        List<CartItem> GetCart();
        void SaveCart(List<CartItem> cart);
    }

}
