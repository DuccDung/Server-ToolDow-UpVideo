using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Server_ToolDow_UpVideo.Models;
using System.IO;

namespace Server_ToolDow_UpVideo.Service
{
    public class YouTubeUploadService : IYouTubeService
    {
        private readonly HttpClient _client;
        private readonly InformationMeetingContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public YouTubeUploadService(IHttpClientFactory factory, InformationMeetingContext context, IConfiguration config, IWebHostEnvironment env)
        {
            _client = factory.CreateClient("Zoom");
            _context = context;
            _config = config;
            _env = env;
        }

        public async Task<TokenResponse> GetAccessToken()
        {
            var token = await RefreshToken();
            if (token.IsSussess && token.Data != null)
            {
                return token.Data;
            }
            else
            {
                throw new Exception(token.Message ?? "Failed to refresh token");
            }
        }
        public async Task<ResponseModel<TokenResponse>> RefreshToken()
        {
            var tokenEntry = await _context.GoogleToken.FirstOrDefaultAsync(x => x.UserId == "duccdung999@gmail.com");
            if (tokenEntry == null || string.IsNullOrEmpty(tokenEntry.RefreshToken))
            {
                return new ResponseModel<TokenResponse>
                {
                    IsSussess = false,
                    Message = "Token entry not found or refresh token is empty",
                    Data = null
                };
            }

            var tokenExpiryTime = tokenEntry.IssuedAt.AddSeconds(tokenEntry.ExpiresIn ?? 0);
            //if (DateTime.UtcNow < tokenExpiryTime.AddMinutes(-15)) 
            //{
            //var googleToken = new TokenResponse
            //{
            //    AccessToken = tokenEntry.AccessToken,
            //    RefreshToken = tokenEntry.RefreshToken,
            //    Scope = tokenEntry.Scope,
            //    ExpiresInSeconds = tokenEntry.ExpiresIn,
            //    IssuedUtc = tokenEntry.IssuedAt
            //};

            //    return new ResponseModel<TokenResponse>
            //    {
            //        IsSussess = true,
            //        Message = "Token refreshed successfully",
            //        Data = googleToken
            //    };
            //}

            var clientSecrets = new ClientSecrets
            {
                ClientId = _config["Google:ClientId"],
                ClientSecret = _config["Google:ClientSecret"]
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets
            });

            var token = new TokenResponse
            {
                RefreshToken = tokenEntry.RefreshToken
            };

            var newToken = await flow.RefreshTokenAsync("duccdung999@gmail.com", token.RefreshToken, CancellationToken.None);

            // Update the new access_token  
            tokenEntry.AccessToken = newToken.AccessToken;
            tokenEntry.ExpiresIn = (int?)newToken.ExpiresInSeconds;
            tokenEntry.IssuedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ResponseModel<TokenResponse>
            {
                IsSussess = true,
                Message = "Token refreshed successfully",
                Data = newToken
            };
        }
        //        public async Task<bool> UploadVideoAsync(string filePath, string title, string description)
        //        {
        //            var token = await GetAccessToken();
        //            var clientSecrets = new ClientSecrets
        //            {
        //                ClientId = _config["Google:ClientId"],
        //                ClientSecret = _config["Google:ClientSecret"]
        //            };

        //            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        //            {
        //                ClientSecrets = clientSecrets,
        //                Scopes = new[] { YouTubeService.Scope.YoutubeUpload },
        //                DataStore = new FileDataStore("YouTubeUploaderTokens")
        //            });

        //            // var credential = new UserCredential(flow, "duccdung999@gmail.com", token);
        //            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        //            new ClientSecrets
        //            {
        //                ClientId = _config["Google:ClientId"],
        //                ClientSecret = _config["Google:ClientSecret"]
        //            },
        //            new[] { YouTubeService.Scope.YoutubeUpload },"duccdung999@gmail.com", CancellationToken.None, new FileDataStore("YouTubeUploaderTokens", true)
        //);


        //            var youtubeService = new Google.Apis.YouTube.v3.YouTubeService(new BaseClientService.Initializer()
        //            {
        //                HttpClientInitializer = credential,
        //                ApplicationName = "Server-ToolDow&UpVideo"
        //            });

        //            var video = new Video
        //            {
        //                Snippet = new VideoSnippet
        //                {
        //                    Title = title,
        //                    Description = description,
        //                    Tags = new[] { "youtube", "asp.net", "upload" },
        //                    CategoryId = "22"
        //                },
        //                Status = new VideoStatus { PrivacyStatus = "unlisted" }
        //            };
        //            try
        //            {
        //                using var stream = new FileStream(filePath, FileMode.Open);
        //                var uploadRequest = youtubeService.Videos.Insert(video, "snippet,status", stream, "video/*");
        //                await uploadRequest.UploadAsync();
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Error uploading video: {ex.Message}");
        //                return false;
        //            }
        //            return true;
        //        }

        public async Task<bool> UploadVideoAsync(string filePath, string title, string description)
        {
            try
            {
                var tokenFolder = Path.Combine(_env.ContentRootPath, "Tokens");
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = _config["Google:ClientId"],
                        ClientSecret = _config["Google:ClientSecret"]
                    },
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "duccdung999@gmail.com", 
                    CancellationToken.None,
                    new FileDataStore(tokenFolder, true)
                );

                var youtubeService = new YouTubeService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Server-ToolDow&UpVideo"
                });

                var video = new Video
                {
                    Snippet = new VideoSnippet
                    {
                        Title = title,
                        Description = description,
                        Tags = new[] { "asp.net", "upload", "youtube" },
                        CategoryId = "22" // 22 = People & Blogs
                    },
                    Status = new VideoStatus { PrivacyStatus = "unlisted" }
                };

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var uploadRequest = youtubeService.Videos.Insert(video, "snippet,status", stream, "video/*");

                var progress = await uploadRequest.UploadAsync();

                if (progress.Status == Google.Apis.Upload.UploadStatus.Completed)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Upload thất bại: " + progress.Exception?.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi upload video: {ex.Message}");
                return false;
            }
        }


        public async Task<List<string>> UploadAllVideosInFolderAsync()
        {
            var uploadResults = new List<string>();
            var videoFolder = Path.Combine(_env.WebRootPath, "videos");

            if (!Directory.Exists(videoFolder))
                return new List<string> { "Folder 'videos' no exist in wwwroot." };

            var files = Directory.GetFiles(videoFolder, "*.mp4");

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                string fileId = Path.GetFileNameWithoutExtension(file);
                var infoUpload = await _context.RecordingFiles.Include(x => x.MeetingUu).FirstOrDefaultAsync(x => x.FileId == fileId);
                try
                {
                    if (infoUpload?.MeetingUu?.Topic != null && !infoUpload.Condition)
                    {
                        var result = await UploadVideoAsync(file, infoUpload.MeetingUu.Topic + " Ngày: " + infoUpload.MeetingUu.StartTime, $"Upload file {fileName}");
                        if (result == true)
                        {
                            infoUpload.Condition = true;
                            uploadResults.Add($"{fileName}: {result}");
                        }
                        else
                        {
                            uploadResults.Add($"{fileName}: false");
                        }
                    }
                }
                catch (Exception ex)
                {
                    uploadResults.Add($"{fileName}:  Error - {ex.Message}");
                }
            }
            return uploadResults;
        }
    }
}
