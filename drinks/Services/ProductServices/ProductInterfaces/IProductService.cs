using Drinks.Models.Entities;
using Drinks.Models.ViewModel;

namespace Drinks.Services.ProductServices.ProductInterfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync(int? brandId = null, decimal? minPrice = null, decimal? maxPrice = null);
        Task<List<Brand>> GetBrandsAsync();
        Task<(decimal MinPrice, decimal MaxPrice)> GetPriceRangeAsync();
        Task<Product?> GetProductAsync(int id);
    }
}
