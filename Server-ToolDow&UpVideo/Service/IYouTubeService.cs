using Server_ToolDow_UpVideo.Models;

namespace Server_ToolDow_UpVideo.Service
{
    public interface IYouTubeService
    {
        Task<List<string>> UploadAllVideosInFolderAsync();
    }
}
