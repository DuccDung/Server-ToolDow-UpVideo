using Server_ToolDow_UpVideo.Models;

namespace Server_ToolDow_UpVideo.Service
{
    public interface IZoomService
    {
        Task<ResponseModel<string>> GetAccessTokenZoom();
        Task<ResponseModel<List<ZoomRecording>>> GetAllRecordingsAsync();
    }
}
