using ProductService.Productdto;

namespace ProductService.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponsedto>> GetAll();
        Task<ProductResponsedto?> GetById(int id);
        Task<ProductResponsedto?> Create(ProductCreatedto dto);
        Task<ProductResponsedto?> Update(ProductUpdatedto dto);
        Task<bool> Delete(int id);
    }
}
