using FFmpeg.VideoStreamDecoder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;

namespace _MediaInfoService.Controllers
{
    [Route("api/rest/v1/[controller]")]
    [ApiController]
    public class ThumbnailController : ControllerBase
    {
        private readonly ILogger<ThumbnailController> _logger;

        public ThumbnailController(ILogger<ThumbnailController> logger)
        {
            this._logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetThumbnail([FromQuery] string url, CancellationToken cancellationToken, [FromQuery] int? frameIndex, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] PngCompressionLevel? compressionLevel)
        {
            this._logger.LogDebug($"ThumbnailController.GetThumbnail: {url}");
            try
            {
                using (Image<Bgr24> image = VideoStreamDecoder.GetThumbnail(url, frameIndex ?? 0, width ?? 0, height ?? 0))
                {
                    var memoryStream = new MemoryStream();
                    //This saves to the memoryStream with encoder
                    await image.SaveAsPngAsync(memoryStream, new PngEncoder() { CompressionLevel = compressionLevel ?? PngCompressionLevel.Level1 }, cancellationToken);
                    memoryStream.Position = 0; // The position needs to be reset.
                    return new FileStreamResult(memoryStream, "image/png")
                    {
                        FileDownloadName = $"{Path.GetFileNameWithoutExtension(url)}_{frameIndex}.png"
                    };
                };
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
