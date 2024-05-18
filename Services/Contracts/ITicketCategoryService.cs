using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos.TicketCategoryDomain;
using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface ITicketCategoryService
    {
        Task<ApiResponse<TicketCategoryDto>> CreateCategory(CreateTicketCatgeory createTicketCatgeory);
        Task<ApiResponse<List<TicketCategoryDto>>> GetCategories();
        Task<ApiResponse<dynamic>> DeleteTicketCategory(string id);
        Task<ApiResponse<TicketCategoryDto>> UpdateCategory(CreateTicketCatgeory updateTicketCatgeory, string id);
        Task<bool> IsExist(Guid id);
        Task<Guid> GetCategoryByApplication(Applications application);
    }
}