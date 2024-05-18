using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos.ProductUpdateDomain;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface IProductUpdatesService
    {
        ApiResponse<IEnumerable<string>> GetPackages();
        Task<ApiResponse<dynamic>> CreateProductUpdateCategory(string category);
        Task<ApiResponse<IEnumerable<ProductUpdateCategoryDto>>> GetProductUpdateCategories();
    }
}
