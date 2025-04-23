using drinks.Models.Entities;

namespace Drinks.Repository.PaymentRepository.PaymentInterfaces
{
    public interface IOrderRepository
    {
        Task AddOrderAsync(Order order);
    }
}
