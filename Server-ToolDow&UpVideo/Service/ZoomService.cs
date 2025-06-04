using Newtonsoft.Json;
using Server_ToolDow_UpVideo.Models;
using System.Net.Http.Headers;
using System.Text;

namespace Server_ToolDow_UpVideo.Service
{
    public class ZoomService : IZoomService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        public ZoomService(IHttpClientFactory factory, IConfiguration configuration)
        {
            _client = factory.CreateClient("Zoom");
            _configuration = configuration;
        }

        public async Task<ResponseModel<string>> GetAccessTokenZoom()
        {
            var clientId = _configuration["Zoom:ClientId"];
            var clientSecret = _configuration["Zoom:ClientSecret"];
            var accountId = _configuration["Zoom:AccountId"];
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var response = await _client.PostAsync($"oauth/token?grant_type=account_credentials&account_id={accountId}", null);
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(json))
            {
                ResponseModel<string> responseModel = new()
                {
                    IsSussess = false,
                    Message = "Failed to retrieve access token."
                };
                return responseModel;
            }
            else
            {
                var tokenObj = JsonConvert.DeserializeObject<RoomTokenResponse>(json);
                if (tokenObj != null)
                {
                    string? token = tokenObj.access_token;
                    int? expires = tokenObj.expires_in;
                    string? type = tokenObj.token_type;
                    return new ResponseModel<string>
                    {
                        IsSussess = true,
                        Message = "Access token retrieved successfully.",
                        Data = token,
                        DataList = new List<string> { type ?? string.Empty, expires?.ToString() ?? string.Empty }
                    };
                }
                return new ResponseModel<string>
                {
                    IsSussess = false,
                    Message = "Failed to parse access token response."
                };
            }
        }

        public Task<ResponseModel<List<ZoomRecording>>> GetAllRecordingsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
