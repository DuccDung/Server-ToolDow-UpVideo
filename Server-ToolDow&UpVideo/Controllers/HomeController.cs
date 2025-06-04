using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server_ToolDow_UpVideo.Service;

namespace Server_ToolDow_UpVideo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IZoomService _zoomService;
        public HomeController(IZoomService zoomService)
        {
            _zoomService = zoomService;
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
    }
}
