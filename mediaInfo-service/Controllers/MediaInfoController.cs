using Microsoft.AspNetCore.Mvc;
using FFmpeg.MediaInfo;
using FFmpeg.MediaInfo.DTOs;
using _MediaInfoService.Extensions;

namespace _MediaInfoService.Controllers
{
    [Route("api/rest/v1/[controller]")]
    [ApiController]
    public class MediaInfoController : ControllerBase
    {
        private readonly ILogger<MediaInfoController> _logger;

        public MediaInfoController(ILogger<MediaInfoController> logger)
        {
            _logger = logger;
        }

        [Produces("application/json")]
        [HttpGet("ffmpeg_version")]
        public IActionResult GetFFmpegVersion()
        {
            return Ok(new
            {
                av_version_info = FFmpeg.AutoGen.ffmpeg.av_version_info(),
                avcodec_version = FFmpeg.AutoGen.ffmpeg.avcodec_version(),
                avdevice_version = FFmpeg.AutoGen.ffmpeg.avdevice_version(),
                avfilter_version = FFmpeg.AutoGen.ffmpeg.avfilter_version(),
                avformat_version = FFmpeg.AutoGen.ffmpeg.avformat_version(),
                avutil_version = FFmpeg.AutoGen.ffmpeg.avutil_version(),
                postproc_version = FFmpeg.AutoGen.ffmpeg.postproc_version(),
                swresample_version = FFmpeg.AutoGen.ffmpeg.swresample_version(),
                swscale_version = FFmpeg.AutoGen.ffmpeg.swscale_version(),

            });
        }

        // POST: api/rest/v1/mediainfo
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(MediaInfoDTO), StatusCodes.Status200OK)]
        [Consumes("text/plain")]
        public IActionResult MediaInfo([FromBody] string url)
        {
            this._logger.LogDebug($"MediaInfo.Processing: { url }");
            try
            {
                MediaInfo mediaInfo = new MediaInfo(url);
                return Ok(mediaInfo.ConvertToDTO());
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
}