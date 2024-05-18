using Grpc.Net.Client;
using Configurations.Utility;
using SuperAdmin.Service.Services.Contracts;
using Serilog;
using ILogger = Serilog.ILogger;

namespace SuperAdmin.Service.Services.Implementations
{
    public class TenantService : ITenantService
    {
        private readonly ILogger _logger = Log.ForContext<TenantService>();
        private readonly GrpcTenantService.GrpcTenantServiceClient _client;
        public TenantService(IConfiguration configuration)
        {
            var address = configuration.GetSection("Services").GetSection("Auth").Value;
            if (address == null) throw new ArgumentNullException(nameof(address));

            var channel = GrpcChannel.ForAddress(address);
            _client = new GrpcTenantService.GrpcTenantServiceClient(channel);
        }

        public async Task<ApiResponse<GetTenantResponse>> GetTenant(string id)
        {
            try
            {
                GetTenantResponse? response = await _client.GetTenantAsync(new GetTenantRequest { Id = id, Subdomain = string.Empty });
                return new ApiResponse<GetTenantResponse>
                {
                    Data = response,
                    ResponseCode = "200",
                    ResponseMessage = $"Tenant fetched successfully!"
                };
            }
            catch (Exception ex)
            {
                string message = "Failed to fetch tenant!";
                _logger.Error(ex, message);
                return new ApiResponse<GetTenantResponse>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }
        }

        public async Task<ApiResponse<List<GetTenantResponse>>> GetTenants()
        {
            try
            {
                GetAllTenantResponse? response = await _client.GetAllTenantsAsync(new GetAllTenantsRequest { Id = string.Empty });
                return new ApiResponse<List<GetTenantResponse>>
                {
                    Data = response.Tenants.ToList(),
                    ResponseCode = "200",
                    ResponseMessage = $"Tenants fetched successfully!"
                };
            }
            catch (Exception ex)
            {
                string message = "Failed to fetch tenants!";
                _logger.Error(ex, message);
                return new ApiResponse<List<GetTenantResponse>>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }
        }
    }
}

