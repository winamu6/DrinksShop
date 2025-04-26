using Drinks.Data;
using Drinks.Models.Entities;
using Drinks.Models.ViewModel;
using Drinks.Repository.ProductRepository.ProductRepositoryInterfaces;
using Drinks.Services.ProductServices.ProductInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Drinks.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IBrandRepository _brandRepo;

        public ProductService(IProductRepository productRepo, IBrandRepository brandRepo)
        {
            _productRepo = productRepo;
            _brandRepo = brandRepo;
        }

        public async Task<List<Product>> GetProductsAsync(int? brandId = null, decimal? minPrice = null, decimal? maxPrice = null)
            => await _productRepo.GetAllAsync(brandId, minPrice, maxPrice);

        public async Task<List<Brand>> GetBrandsAsync()
            => await _brandRepo.GetAllAsync();

        public async Task<(decimal MinPrice, decimal MaxPrice)> GetPriceRangeAsync()
        {
            if (!await _productRepo.AnyAsync())
                return (0, 100);

            var min = await _productRepo.GetMinPriceAsync() ?? 0;
            var max = await _productRepo.GetMaxPriceAsync() ?? 100;

            return (min, max);
        }

        public async Task<Product?> GetProductAsync(int id)
            => await _productRepo.GetByIdAsync(id);
    }

}
