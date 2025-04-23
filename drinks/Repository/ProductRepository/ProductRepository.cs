using drinks.Data;
using drinks.Models.Entities;
using drinks.Models.ViewModel;
using Drinks.Repository.ProductRepository.ProductRepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Drinks.Repository.ProductRepository
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync(int? brandId = null, decimal? minPrice = null, decimal? maxPrice = null)
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

        public async Task<Product?> GetByIdAsync(int id) =>
            await _context.Products.Include(p => p.Brand).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<decimal?> GetMinPriceAsync() =>
            await _context.Products.MinAsync(p => (decimal?)p.Price);

        public async Task<decimal?> GetMaxPriceAsync() =>
            await _context.Products.MaxAsync(p => (decimal?)p.Price);

        public async Task<bool> AnyAsync() =>
            await _context.Products.AnyAsync();
    }

    public class BrandRepository : IBrandRepository
    {
        private readonly AppDbContext _context;

        public BrandRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Brand>> GetAllAsync() =>
            await _context.Brands.ToListAsync();
    }

}
