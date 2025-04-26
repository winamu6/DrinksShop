using Drinks.Data;
using Drinks.Models;
using Drinks.Models.Entities;
using Drinks.Models.ViewModel;
using Drinks.Repository.PaymentRepository.PaymentInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Drinks.Services.PaymentServices
{
    public class PaymentService
    {
        private readonly IPaymentProductRepository _productRepository;
        private readonly ICoinRepository _coinRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly AppDbContext _context;

        public PaymentService(
            IPaymentProductRepository productRepository,
            ICoinRepository coinRepository,
            IOrderRepository orderRepository,
            AppDbContext context)
        {
            _productRepository = productRepository;
            _coinRepository = coinRepository;
            _orderRepository = orderRepository;
            _context = context;
        }

        public async Task<PaymentResult> ProcessPayment(PaymentRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in request.CartItems)
                {
                    var product = await _productRepository.GetProductAsync(item.ProductId);
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
                    var availableCoins = await _coinRepository.GetAvailableCoinsAsync();

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
                            await _coinRepository.UpdateCoinAsync(coin);
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
                    var product = await _productRepository.GetProductAsync(item.ProductId);
                    product.Quantity -= item.Quantity;
                    await _productRepository.UpdateProductQuantityAsync(product);
                }

                foreach (var coin in request.InsertedCoins)
                {
                    var dbCoin = await _coinRepository.GetCoinByDenominationAsync(coin.Key);
                    if (dbCoin != null)
                    {
                        dbCoin.Count += coin.Value;
                        await _coinRepository.UpdateCoinAsync(dbCoin);
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

                await _orderRepository.AddOrderAsync(order);
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