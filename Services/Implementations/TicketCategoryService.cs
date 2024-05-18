using System.Data;
using System.Globalization;
using Configurations.Utility;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Models.Dtos.TicketCategoryDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.Implementations
{
    public class TicketCategoryService : ITicketCategoryService
    {
        private readonly AppDbContext _appDbContext;

        public TicketCategoryService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// This method creates a new ticket category 
        /// </summary>
        /// <param name="createTicketCatgeory"></param>
        public async Task<ApiResponse<TicketCategoryDto>> CreateCategory(CreateTicketCatgeory createTicketCatgeory)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            if (await CheckExistingName(createTicketCatgeory.CategoryName))
            {
                return new ApiResponse<TicketCategoryDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Ticket category name '{createTicketCatgeory.CategoryName}' already exists!"
                };
            }

            TicketCategory category = new() { Name = textInfo.ToTitleCase(createTicketCatgeory.CategoryName.Trim().ToLower()) };
            await _appDbContext.AddAsync(category);
            await _appDbContext.SaveChangesAsync();
            return new ApiResponse<TicketCategoryDto>
            {
                Data = new TicketCategoryDto
                {
                    Id = category.Id,
                    CategoryName = category.Name
                },
                ResponseCode = "200",
                ResponseMessage = $"Created {category.Name} category successfully!"
            };
        }

        /// <summary>
        /// This method deletes a ticket category
        /// </summary>
        /// <param name="id"></param>
        public async Task<ApiResponse<dynamic>> DeleteTicketCategory(string id)
        {
            TicketCategory? ticketCategory = await _appDbContext.TicketCategories.FindAsync(id);
            if (ticketCategory == null)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Ticket category does not exist"
                };
            }

            ticketCategory.IsDeleted = true;
            _appDbContext.TicketCategories.Update(ticketCategory);
            await _appDbContext.SaveChangesAsync();
            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Deleted ticket category successfully"
            };
        }

        /// <summary>
        /// This method gets all ticket categories
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<List<TicketCategoryDto>>> GetCategories()
        {
            List<TicketCategoryDto> categories = await _appDbContext.TicketCategories
                .Where(t => !t.IsDeleted)
                .Select(tc => new TicketCategoryDto
                {
                    CategoryName = tc.Name,
                    Id = tc.Id
                })
                .ToListAsync();

            return new ApiResponse<List<TicketCategoryDto>>
            {
                Data = categories,
                ResponseCode = "200",
                ResponseMessage = "Categories fetched successfully!"
            };
        }

        public async Task<Guid> GetCategoryByApplication(Applications application)
        {
            return await _appDbContext.TicketCategories.Where(tc => tc.Name.Equals(application)).Select(tc => tc.Id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// This method check an existing ticket category by id
        /// </summary>
        /// <param name="id"></param>
        public async Task<bool> IsExist(Guid id)
        {
            return await _appDbContext.TicketCategories
                .Where(tc => tc.Id.Equals(id))
                .AnyAsync();
        }

        /// <summary>
        /// This method updates an existing ticket category 
        /// </summary>
        /// <param name="updateTicketCatgeory"></param>
        /// <param name="id"></param>
        public async Task<ApiResponse<TicketCategoryDto>> UpdateCategory(CreateTicketCatgeory updateTicketCatgeory, string id)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            TicketCategory? ticketCategory = await _appDbContext.TicketCategories.FindAsync(id);
            if (ticketCategory == null)
            {
                return new ApiResponse<TicketCategoryDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Ticket category does not exist"
                };
            }

            if (await CheckExistingName(updateTicketCatgeory.CategoryName))
            {
                return new ApiResponse<TicketCategoryDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Ticket category name '{updateTicketCatgeory.CategoryName}' already exists!"
                };
            }

            ticketCategory.Name = textInfo.ToTitleCase(updateTicketCatgeory.CategoryName.Trim().ToLower());
            _appDbContext.TicketCategories.Update(ticketCategory);
            await _appDbContext.SaveChangesAsync();
            return new ApiResponse<TicketCategoryDto>
            {
                Data = new TicketCategoryDto
                {
                    Id = ticketCategory.Id,
                    CategoryName = ticketCategory.Name
                },
                ResponseCode = "200",
                ResponseMessage = $"Updated {ticketCategory.Name} category successfully!"
            };
        }

        /// <summary>
        /// This method check an existing ticket category name
        /// </summary>
        /// <param name="categoryName"></param>
        private Task<bool> CheckExistingName(string categoryName)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return _appDbContext.TicketCategories
                .Where(tc => tc.Name.Equals(textInfo.ToTitleCase(categoryName.Trim().ToLower())))
                .AnyAsync();
        }
    }
}

