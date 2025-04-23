using drinks.Models.ViewModel;

namespace Drinks.Repository.ProductRepository.ProductRepositoryInterfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync(int? brandId = null, decimal? minPrice = null, decimal? maxPrice = null);
        Task<Product?> GetByIdAsync(int id);
        Task<decimal?> GetMinPriceAsync();
        Task<decimal?> GetMaxPriceAsync();
        Task<bool> AnyAsync();
    }
}
