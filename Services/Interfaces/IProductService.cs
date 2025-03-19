using Data.Entities;
using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetAvailableProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(ProductRequest productDto, string? tokenRole);

        Task<ProductDto?> UpdateProductAsync(int id, ProductDto productDto);

        Task<ProductDto?> DeleteProductAsync(int id);
        Task ApproveProduct(int productId, bool approve);
    }
}
