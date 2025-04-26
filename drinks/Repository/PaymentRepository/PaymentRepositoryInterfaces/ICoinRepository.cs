using Drinks.Models.Entities;

namespace Drinks.Repository.PaymentRepository.PaymentInterfaces
{
    public interface ICoinRepository
    {
        Task<List<Coin>> GetAvailableCoinsAsync();
        Task<Coin> GetCoinByDenominationAsync(int denomination);
        Task UpdateCoinAsync(Coin coin);
    }
}
