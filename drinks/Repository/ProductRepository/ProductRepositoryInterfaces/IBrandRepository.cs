using drinks.Models.Entities;

namespace Drinks.Repository.ProductRepository.ProductRepositoryInterfaces
{
    public interface IBrandRepository
    {
        Task<List<Brand>> GetAllAsync();
    }
}
