using drinks.Models.ViewModel;

namespace Drinks.Repository.PaymentRepository.PaymentInterfaces
{
    public interface IPaymentProductRepository
    {
        Task<Product> GetProductAsync(int productId);
        Task UpdateProductQuantityAsync(Product product);
    }
}
