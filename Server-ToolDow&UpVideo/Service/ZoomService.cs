using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Server_ToolDow_UpVideo.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using ToolDowloadVideoAndUpToYoutube.Models;

namespace Server_ToolDow_UpVideo.Service
{
    public class ZoomService : IZoomService
    {
        private readonly HttpClient _client;
        private readonly HttpClient _clientDownload;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly InformationMeetingContext _context;
        private readonly IWebHostEnvironment _env;
        public ZoomService(IHttpClientFactory factory, IConfiguration configuration, IMemoryCache memoryCache, InformationMeetingContext context, IWebHostEnvironment env)
        {
            _client = factory.CreateClient("Zoom");
            _clientDownload = factory.CreateClient("ZoomDownload");
            _configuration = configuration;
            _cache = memoryCache;
            _context = context;
            _env = env;
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
                    _cache.Set("zoom_access_token", token, TimeSpan.FromMinutes(55));
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
        public async Task<bool> IsRecordingExist(string fileId)
        {
            var isRecordingExist = await _context.RecordingFiles.AnyAsync(m => m.FileId == fileId);
            if (isRecordingExist)
            {
                return true; // recording already exists
            }
            return false;
        }
        public async Task<bool> IsMeetingExist(string UuId)
        {
            var isUuidExist = await _context.Meetings.AnyAsync(m => m.Uuid == UuId);
            if (isUuidExist)
            {
                return true; // Meeting already exists
            }
            return false;
        }
        public static string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
        public async Task<ResponseModel<ZoomRecordingFile>> GetNewRecordingsAsync()
        {
            var isrefreshToken = await RefeshAccessTokenZoom();
            if (!isrefreshToken.IsSussess)
            {
                return new ResponseModel<ZoomRecordingFile>
                {
                    IsSussess = false,
                    Message = isrefreshToken.Message
                };
            }
            // Check if the access token is cached
            var accessToken = _cache.Get<string>("zoom_access_token");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Get the current date in UTC format
            var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var response = await _client.GetAsync($"v2/users/me/recordings?from=2024-01-01&to={toDate}&page_size=100");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(json))
                {
                    return new ResponseModel<ZoomRecordingFile>
                    {
                        IsSussess = false,
                        Message = "No recordings found."
                    };
                }
                else
                {
                    var meetings = JsonConvert.DeserializeObject<ZoomRecordingListResponse>(json);
                    if (meetings?.Meetings == null || meetings.Meetings.Count < 1)
                    {
                        return new ResponseModel<ZoomRecordingFile>
                        {
                            IsSussess = false,
                            Message = "No recordings found."
                        };
                    }
                    else
                    {
                        var responseModel = new ResponseModel<ZoomRecordingFile>
                        {
                            IsSussess = true,
                            Message = "Recordings retrieved successfully.",
                            DataList = new List<ZoomRecordingFile>()
                        };

                        foreach (var meeting in meetings.Meetings)
                        {
                            if (meeting != null)
                            {
                                // Process each meeting (implementation not provided in the original code)
                                string uuid = meeting.Uuid ?? string.Empty;
                                var recordings = await _client.GetAsync($"v2/meetings/{uuid}/recordings");
                                if (recordings.IsSuccessStatusCode)
                                {
                                    var recordingJson = await recordings.Content.ReadAsStringAsync();
                                    var recordingData = JsonConvert.DeserializeObject<ZoomMeetingRecordingDetail>(recordingJson);
                                    if (recordingData?.RecordingFiles == null) continue;
                                    // check if recording files exist
                                    if (await IsMeetingExist(uuid) == false && !string.IsNullOrEmpty(uuid))
                                    {
                                        await _context.Meetings.AddAsync(new Meeting
                                        {
                                            Uuid = uuid,
                                            Topic = recordingData.Topic,
                                            StartTime = recordingData.StartTime,
                                        });
                                    }
                                    foreach (var file in recordingData.RecordingFiles)
                                    {
                                        if (file != null && !string.IsNullOrEmpty(file.DownloadUrl) && file.FileType == "MP4" && !string.IsNullOrEmpty(file.Id))
                                        {
                                            // Here you can process the file, e.g., download it or store its information
                                            // For now, we just return the first file's download URL
                                            if (await IsRecordingExist(file.Id) == false)
                                            {
                                                await _context.AddAsync(new RecordingFile
                                                {
                                                    FileId = file.Id,
                                                    FileName = file.FileType,
                                                    DownloadUrl = file.DownloadUrl,
                                                    MeetingUuid = uuid,
                                                    DownloadedAt = null,
                                                    FileType = file.FileType
                                                });
                                            }
                                            var recordingFile = new ZoomRecordingFile()
                                            {
                                                Id = file.Id,
                                                FileType = file.FileType,
                                                FileSize = file.FileSize,
                                                DownloadUrl = file.DownloadUrl,
                                                PlayUrl = file.PlayUrl,
                                                RecordingStart = file.RecordingStart,
                                                RecordingEnd = file.RecordingEnd
                                            };
                                            responseModel.DataList.Add(recordingFile);
                                        }
                                    }
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                        return responseModel;
                    }
                }
            }
            return new ResponseModel<ZoomRecordingFile>
            {
                IsSussess = false,
                Message = "No recordings found."
            };
        }
        public async Task<ResponseModel<string>> RefeshAccessTokenZoom()
        {
            if (_cache.TryGetValue("zoom_access_token", out object? cachedValue) && cachedValue is string accessToken)
            {
                return await Task.FromResult(new ResponseModel<string>
                {
                    IsSussess = true,
                    Message = "Access token is still valid.",
                    Data = accessToken
                });
            }
            else
            {
                var response = await GetAccessTokenZoom();
                if (response.IsSussess)
                {
                    // Cache the new access token
                    _cache.Set("zoom_access_token", response.Data, TimeSpan.FromMinutes(55));
                    return response;
                }
            }
            return new ResponseModel<string>
            {
                IsSussess = false,
                Message = "Failed to refresh access token."
            };
        }
        public async Task<ResponseModel<string>> SaveRecordingToServerAsync(string downloadUrl, string fileName)
        {
            try
            {
                var isrefreshToken = await RefeshAccessTokenZoom();
                if (!isrefreshToken.IsSussess)
                {
                    return new ResponseModel<string>
                    {
                        IsSussess = false,
                        Message = isrefreshToken.Message
                    };
                }
                // Check if the access token is cached
                var accessToken = _cache.Get<string>("zoom_access_token");

                fileName = SanitizeFileName(fileName);
                string savePath = Path.Combine(_env.WebRootPath, "videos");
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                var separator = downloadUrl.Contains("?") ? "&" : "?";
                string fullDownloadUrl = $"{downloadUrl}{separator}access_token={accessToken}";
                var response = await _clientDownload.GetAsync(fullDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    string filePath = Path.Combine(savePath, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    return new ResponseModel<string>
                    {
                        IsSussess = true,
                        Message = "Recording saved successfully.",
                        Data = filePath
                    };
                }
                else
                {
                    return new ResponseModel<string>
                    {
                        IsSussess = false,
                        Message = $"Failed to download recording. Status code: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<string>
                {
                    IsSussess = false,
                    Message = $"Error saving recording to database: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel<string>> SaveNewRecordingsAsync()
        {
            var listLink = await _context.RecordingFiles.Include(m => m.MeetingUu)
                .Where(m => m.DownloadedAt == null && m.MeetingUu != null)
                .Select(m => new { m.DownloadUrl, Topic = m.MeetingUu!.Topic ?? string.Empty , FileId = m.FileId })
                .ToListAsync();
            List<string> listDownloadUrlsFail = new List<string>();
            foreach (var item in listLink)
            {
                if (!string.IsNullOrEmpty(item.DownloadUrl))
                {
                    var checkSave = await SaveRecordingToServerAsync(item.DownloadUrl, $"{item.FileId}.mp4");
                    if (!checkSave.IsSussess)
                    {
                        listDownloadUrlsFail.Add(item.DownloadUrl);
                    }
                    else
                    {
                        var recordingFile = await _context.RecordingFiles.FirstOrDefaultAsync(m => m.DownloadUrl == item.DownloadUrl);
                        if (recordingFile != null)
                        {
                            recordingFile.DownloadedAt = DateTime.Now;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            return new ResponseModel<string>
            {
                IsSussess = true,
                Message = "All recordings processed successfully.",
                DataList = listDownloadUrlsFail
            };
        }
    }
}
