using Configurations.Utility;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Database.Enums;
using SuperAdmin.Service.Models.Dtos.ProductUpdateDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.Implementations
{
    public class ProductUpdatesService : IProductUpdatesService
    {
        private readonly AppDbContext _appDbContext;

        public ProductUpdatesService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// This method creates a new product update category
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<dynamic>> CreateProductUpdateCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide category name"
                };
            }

            string trimmedCategory = category.Trim().ToLower();
            var existingCategory = await _appDbContext.ProductUpdateCategories.FirstOrDefaultAsync(x => x.Name == trimmedCategory);
            
            if (existingCategory is not null && existingCategory.IsDeleted)
            {
                existingCategory.IsDeleted = true;
                existingCategory.ModifiedAt = DateTime.UtcNow;

                await _appDbContext.SaveChangesAsync();
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "200",
                    ResponseMessage = "Created category successfully"
                };
            }
            else if (existingCategory is not null && !existingCategory.IsDeleted)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseMessage = "Category already exists",
                    ResponseCode = "200"
                };
            }
            else
            {
                _appDbContext.ProductUpdateCategories.Add(new ProductUpdateCategory
                {
                    Name = trimmedCategory,
                });
                
                await _appDbContext.SaveChangesAsync();
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "200",
                    ResponseMessage = "Created category successfully"
                };
            }
        }

        /// <summary>
        /// This method gets all job suite packages
        /// </summary>
        /// <returns></returns>
        public ApiResponse<IEnumerable<string>> GetPackages()
        {
            var packages = Enum.GetNames(typeof(JobSuitePackage)).ToList();
            return new ApiResponse<IEnumerable<string>>
            {
                Data = packages,
                ResponseCode = "200"
            };
        }

        /// <summary>
        /// This method gets all product update categories
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<IEnumerable<ProductUpdateCategoryDto>>> GetProductUpdateCategories()
        {
            var productCategories = await _appDbContext.ProductUpdateCategories.Where(x => !x.IsDeleted)
                                                                                                        .Select(x => new ProductUpdateCategoryDto
                                                                                                        {
                                                                                                            CategoryId = x.Id,
                                                                                                            CategoryName = x.Name
                                                                                                        }).ToListAsync();

            return new ApiResponse<IEnumerable<ProductUpdateCategoryDto>>
            {
                Data = productCategories,
                ResponseCode = "200"
            };
        }
    }
}
