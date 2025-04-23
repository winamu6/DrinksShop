using drinks.Data;
using drinks.Models.Entities;
using drinks.Models.ViewModel;
using Drinks.Services.ProductServices.ProductInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Drinks.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetProductsAsync(int? brandId = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var query = _context.Products.Include(p => p.Brand).AsQueryable();

            if (brandId.HasValue && brandId > 0)
                query = query.Where(p => p.BrandId == brandId);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);

            return await query.ToListAsync();
        }

        public async Task<List<Brand>> GetBrandsAsync() =>
            await _context.Brands.ToListAsync();

        public async Task<(decimal MinPrice, decimal MaxPrice)> GetPriceRangeAsync()
        {
            try
            {
                if (!await _context.Products.AnyAsync())
                {
                    return (MinPrice: 0, MaxPrice: 100);
                }

                var min = await _context.Products.MinAsync(p => p.Price);
                var max = await _context.Products.MaxAsync(p => p.Price);
                return (MinPrice: min, MaxPrice: max);
            }
            catch
            {
                return (MinPrice: 0, MaxPrice: 100);
            }
        }

        public async Task<Product?> GetProductAsync(int id) =>
            await _context.Products.Include(p => p.Brand).FirstOrDefaultAsync(p => p.Id == id);
    }
}
