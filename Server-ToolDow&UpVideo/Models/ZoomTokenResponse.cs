using Newtonsoft.Json;

namespace Server_ToolDow_UpVideo.Models
{
    public class ZoomTokenResponse
    {
        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string? TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("scope")]
        public string? Scope { get; set; }

        [JsonProperty("api_url")]
        public string? ApiUrl { get; set; }
    }

}
