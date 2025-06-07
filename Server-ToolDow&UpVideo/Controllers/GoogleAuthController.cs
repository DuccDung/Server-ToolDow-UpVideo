using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server_ToolDow_UpVideo.Models;

namespace Server_ToolDow_UpVideo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleAuthController : ControllerBase
    {
        private readonly InformationMeetingContext _context;
        private readonly IConfiguration _config;

        public GoogleAuthController(IConfiguration config, InformationMeetingContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpGet]
        [Route("LoginWithGoogle")]
        public IActionResult LoginWithGoogle()
        {
            var clientId = _config["Google:ClientId"];
            var redirectUri = _config["Google:RedirectUri"];
            var scope = "https://www.googleapis.com/auth/youtube.upload";

            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&access_type=offline&prompt=consent";
            return Redirect(authUrl);
        }
        [HttpGet]
        [Route("oauth2callback")]
        public async Task<IActionResult> OAuth2Callback(string code)
        {
            var clientSecrets = new ClientSecrets
            {
                ClientId = _config["Google:ClientId"],
                ClientSecret = _config["Google:ClientSecret"]
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
                Scopes = new[] { YouTubeService.Scope.YoutubeUpload },
                DataStore = new FileDataStore("YouTubeUploaderTokens")
            });

            // Đổi code lấy token  
            var token = await flow.ExchangeCodeForTokenAsync(
                userId: "me",
                code: code,
                redirectUri: _config["Google:RedirectUri"],
                taskCancellationToken: CancellationToken.None
            );

            // Kiểm tra xem đã có refresh_token hay chưa  
            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                var existing = await _context.GoogleToken.FirstOrDefaultAsync(x => x.UserId == "me");
                if (existing == null)
                {
                    var googleToken = new GoogleToken
                    {
                        UserId = "me", // hoặc bạn lấy email từ userinfo API nếu cần  
                        AccessToken = token.AccessToken,
                        RefreshToken = token.RefreshToken,
                        TokenType = token.TokenType,
                        Scope = token.Scope,
                        ExpiresIn = (int?)(token.ExpiresInSeconds ?? 3600), // Explicit cast added here  
                        IssuedAt = DateTime.UtcNow
                    };
                    _context.GoogleToken.Add(googleToken);
                }
                else
                {
                    existing.AccessToken = token.AccessToken;
                    existing.RefreshToken = token.RefreshToken;
                    existing.ExpiresIn = (int?)(token.ExpiresInSeconds ?? 3600); // Explicit cast added here  
                    existing.IssuedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return Ok("Đăng nhập Google thành công. Refresh Token đã được lưu.");
        }
    }
}
