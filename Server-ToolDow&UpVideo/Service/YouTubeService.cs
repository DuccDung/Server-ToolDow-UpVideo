namespace Server_ToolDow_UpVideo.Service
{
    public class YouTubeService : IYouTubeService
    {
        private readonly HttpClient _client;
        public YouTubeService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("Zoom");
        }
    }
}
