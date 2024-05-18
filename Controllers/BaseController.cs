using Configurations.Utility;
using Microsoft.AspNetCore.Mvc;

namespace SuperAdmin.Service.Controllers
{
    [Produces("application/json")]
    public class BaseController : ControllerBase
    {
        [NonAction]
        [ApiExplorerSettings(IgnoreApi = true)]
        protected IActionResult ParseResponse<T>(ApiResponse<T> response) where T : class
        {
            switch (response.ResponseCode)
            {
                case "200":
                    return StatusCode(200, response);
                case "401":
                    return StatusCode(401, response);
                case "403":
                    return StatusCode(403, response);
                case "404":
                    return StatusCode(404, response);
                case "400":
                    return StatusCode(400, response);
                case "201":
                    return StatusCode(201, response);
                case "409":
                    return StatusCode(409, response);
                default:
                    return StatusCode(500, "Unexpected failure");
            }
        }
    }
}
