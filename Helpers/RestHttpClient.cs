using Configurations.Utility;
using Newtonsoft.Json;
using RestSharp;

namespace SuperAdmin.Service.Helpers
{
    public class RestHttpClient
    {
        private readonly IConfiguration _config;
        private readonly ILogger<RestHttpClient> _logger;

        public RestHttpClient(IConfiguration config, ILogger<RestHttpClient> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<ApiResponse<T>> MakeGenericRequestCall<T>(Method httpVerb, string url, object? requestBody, string? token, string? idempotencyRef = null)
        {
            var restRequest = new RestRequest();
            restRequest.Method = httpVerb;

            var apiKey = _config["WeavrConfigurations:API_KEY"];
            url = _config["WeavrConfigurations:BASE_URL"] + url;

            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddHeader("api-key", apiKey!);

            if (!string.IsNullOrWhiteSpace(token))
            {
                restRequest.AddHeader("Authorization", $"Bearer {token}");
            }

            if (!string.IsNullOrWhiteSpace(idempotencyRef))
            {
                restRequest.AddHeader("idempotency-ref", idempotencyRef);
            }

            if (httpVerb != Method.Get && requestBody != null)
            {
                restRequest.AddBody(requestBody);
            }

            using (var client = new RestClient(url))
            {
                var response = await client.ExecuteAsync(restRequest);

                if (response.IsSuccessful)
                {
                    return new ApiResponse<T>
                    {
                        Data = JsonConvert.DeserializeObject<T>(response.Content),
                        ResponseCode = "200",
                    };
                }
                else
                {
                    var payload = JsonConvert.SerializeObject(requestBody);
                    _logger.LogError($"payload: {payload} --- URL: {url} --- Error Message: {response.Content}");

                    return new ApiResponse<T>
                    {
                        ResponseMessage = response.Content,
                        ResponseCode = ((int)response.StatusCode).ToString(),
                    };
                }
            }
        }
    }
}
