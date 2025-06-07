using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server_ToolDow_UpVideo.Models;
using Server_ToolDow_UpVideo.Service;
using System.Reflection.Metadata.Ecma335;

namespace Server_ToolDow_UpVideo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IZoomService _zoomService;
        private readonly IYouTubeService _youTubeService;
        private readonly InformationMeetingContext _context;
        private readonly IWebHostEnvironment _env;
        public HomeController(IZoomService zoomService, InformationMeetingContext context, IYouTubeService youTubeService, IWebHostEnvironment env)
        {
            _zoomService = zoomService;
            _context = context;
            _youTubeService = youTubeService;
            _env = env;
        }
        [HttpGet]
        [Route("GetAccessTokenZoom")]
        public async Task<IActionResult> GetAccessTokenZoom()
        {
            var response = await _zoomService.GetAccessTokenZoom();
            if (response.IsSussess)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        [HttpGet]
        [Route("GetNewRecordingsAsync")]
        public async Task<IActionResult> GetNewRecordingsAsync()
        {
            var response = await _zoomService.GetNewRecordingsAsync();
            if (response.IsSussess)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        [HttpGet]
        [Route("SaveRecordingToDatabaseAsync")]
        public async Task<IActionResult> SaveRecordingToDatabaseAsync()
        {
            var response = await _zoomService.SaveNewRecordingsAsync();
            if (response.IsSussess)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        [HttpGet]
        [Route("Check")]
        public async Task<IActionResult> Check()
        {
            var recordings = await _context.RecordingFiles.ToListAsync();
            return Ok(recordings);
        }

        [HttpGet]
        [Route("UploadVideo")]
        public async Task<IActionResult> UploadVideo()
        {
            var response = await _youTubeService.UploadAllVideosInFolderAsync();
            return Ok(response);
        }
    }
}