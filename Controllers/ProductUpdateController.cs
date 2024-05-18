using Configurations.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos.ProductUpdateDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/v1/product-update")]
    [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductUpdateController : BaseController
    {
        private readonly IProductUpdatesService _productUpdateService;

        public ProductUpdateController(IProductUpdatesService productUpdateService)
        {
            _productUpdateService = productUpdateService;
        }

        /// <summary>
        /// This endpoint gets all job suite packages
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("packages")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        public IActionResult GetPackages()
        {
            var response = _productUpdateService.GetPackages();
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a new category for product updates
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        [HttpPost("categories/{categoryName}")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        public async Task<IActionResult> CreateProductUpdateCategory([FromRoute] string categoryName)
        {
            var response = await _productUpdateService.CreateProductUpdateCategory(categoryName);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets all product update categories
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductUpdateCategoryDto>>), 200)]
        public async Task<IActionResult> GetProductUpdateCategories()
        {
            var response = await _productUpdateService.GetProductUpdateCategories();
            return ParseResponse(response);
        }
    }
}
