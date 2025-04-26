using Drinks.Models.Entities;

namespace Drinks.Repository.ProductRepository.ProductRepositoryInterfaces
{
    public interface IBrandRepository
    {
        Task<List<Brand>> GetAllAsync();
    }
}
