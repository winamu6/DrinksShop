using drinks.Models.ViewModel;

namespace Drinks.Repository.CartRepository.CartInterfaces
{
    public interface ICartRepository
    {
        List<CartItem> GetCart();
        void SaveCart(List<CartItem> cart);
    }

}
