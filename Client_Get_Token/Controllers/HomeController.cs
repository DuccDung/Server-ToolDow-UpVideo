using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Client_Get_Token.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Client_Get_Token.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InformationMeetingContext _context;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger , InformationMeetingContext context , IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }

        [HttpGet]
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

            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                var existing = await _context.GoogleTokens.FirstOrDefaultAsync(x => x.UserId == "duccdung999@gmail.com");
                if (existing != null)
                {
                    existing.AccessToken = token.AccessToken;
                    existing.RefreshToken = token.RefreshToken;
                    existing.TokenType = token.TokenType;
                    existing.Scope = token.Scope;
                    existing.ExpiresIn = (int?)(token.ExpiresInSeconds ?? 3600);
                    existing.IssuedAt = DateTime.UtcNow;

                    _context.GoogleTokens.Update(existing);
                    await _context.SaveChangesAsync();
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

            return Content("Cập nhập Google thành công. Refresh Token đã được lưu.");
        }
        public IActionResult Index()
        {
            
            return View();
        }
    }
}
