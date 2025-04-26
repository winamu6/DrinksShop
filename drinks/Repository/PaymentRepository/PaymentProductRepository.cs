using Drinks.Data;
using Drinks.Models.Entities;
using Drinks.Models.ViewModel;
using Drinks.Repository.PaymentRepository.PaymentInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Drinks.Repository.PaymentRepository
{
    public class PaymentProductRepository : IPaymentProductRepository
    {
        private readonly AppDbContext _context;

        public PaymentProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetProductAsync(int productId)
            => await _context.Products.FindAsync(productId);

        public async Task UpdateProductQuantityAsync(Product product)
            => _context.Products.Update(product);
    }

    public class CoinRepository : ICoinRepository
    {
        private readonly AppDbContext _context;

        public CoinRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Coin>> GetAvailableCoinsAsync()
            => await _context.Coins.Where(c => c.Count > 0).OrderByDescending(c => c.Denomination).ToListAsync();

        public async Task<Coin> GetCoinByDenominationAsync(int denomination)
            => await _context.Coins.FirstOrDefaultAsync(c => c.Denomination == denomination);

        public async Task UpdateCoinAsync(Coin coin)
            => _context.Coins.Update(coin);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddOrderAsync(Order order)
            => await _context.Orders.AddAsync(order);
    }

}
