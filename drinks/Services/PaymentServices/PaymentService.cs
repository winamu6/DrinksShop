using drinks.Data;
using drinks.Models;
using drinks.Models.Entities;
using drinks.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Drinks.Services.PaymentServices
{
    public class PaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentResult> ProcessPayment(PaymentRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in request.CartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        return new PaymentResult
                        {
                            Success = false,
                            Message = $"Товар '{item.ProductName}' недоступен"
                        };
                    }
                }

                decimal totalAmount = request.CartItems.Sum(i => i.Price * i.Quantity);
                decimal insertedAmount = request.InsertedCoins.Sum(c => c.Key * c.Value);

                if (insertedAmount < totalAmount)
                {
                    return new PaymentResult
                    {
                        Success = false,
                        Message = $"Недостаточно средств. Нужно ещё {totalAmount - insertedAmount} руб."
                    };
                }

                decimal changeAmount = insertedAmount - totalAmount;
                var changeCoins = new Dictionary<int, int>();

                if (changeAmount > 0)
                {
                    var availableCoins = await _context.Coins
                        .Where(c => c.Count > 0)
                        .OrderByDescending(c => c.Denomination)
                        .ToListAsync();

                    decimal remainingChange = changeAmount;

                    foreach (var coin in availableCoins)
                    {
                        if (remainingChange <= 0) break;

                        int maxCoins = (int)(remainingChange / coin.Denomination);
                        int coinsToTake = Math.Min(maxCoins, coin.Count);

                        if (coinsToTake > 0)
                        {
                            changeCoins.Add(coin.Denomination, coinsToTake);
                            remainingChange -= coin.Denomination * coinsToTake;
                            coin.Count -= coinsToTake;
                        }
                    }

                    if (remainingChange > 0)
                    {
                        return new PaymentResult
                        {
                            Success = false,
                            Message = "Не можем выдать точную сдачу. Попробуйте другую комбинацию монет."
                        };
                    }
                }

                foreach (var item in request.CartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Quantity -= item.Quantity;
                    }
                }

                foreach (var coin in request.InsertedCoins)
                {
                    var dbCoin = await _context.Coins
                        .FirstOrDefaultAsync(c => c.Denomination == coin.Key);

                    if (dbCoin != null)
                    {
                        dbCoin.Count += coin.Value;
                    }
                }

                var order = new Order
                {
                    Date = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    OrderItems = request.CartItems.Select(i => new OrderItem
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        BrandName = i.BrandName,
                        PriceAtOrderTime = i.Price
                    }).ToList()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new PaymentResult
                {
                    Success = true,
                    ChangeAmount = changeAmount,
                    ChangeCoins = changeCoins,
                    Order = order
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new PaymentResult
                {
                    Success = false,
                    Message = $"Ошибка при обработке платежа: {ex.InnerException?.Message ?? ex.Message}"
                };
            }
        }
    }
}