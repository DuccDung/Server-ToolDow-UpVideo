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
        private readonly InformationMeetingContext _context;
        public HomeController(IZoomService zoomService , InformationMeetingContext context)
        {
            _zoomService = zoomService;
            _context = context;
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
            return Ok(await _context.RecordingFiles.ToListAsync());   
        }
    }
}
