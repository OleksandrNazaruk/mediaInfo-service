using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using _MediaInfoService.Models;
using System.Text;
using FFmpeg.VolumeDetect;
using FFmpeg.VolumeDetect.Models;
using FFmpeg.VolumeDetect.DTOs;
using _MediaInfoService.Extensions;

namespace _MediaInfoService.Controllers
{
    [Route("api/rest/v1/[controller]")]
    [ApiController]
    public class VolumeDetectController : ControllerBase
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly ILogger<VolumeDetectController> _logger;

        public VolumeDetectController(ILogger<VolumeDetectController> logger, IBackgroundTaskQueue queue)
        {
            this._queue = queue;
            this._logger = logger;
        }


        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(BackgroundTaskDTO), StatusCodes.Status200OK)]
        [Consumes("text/plain")]
        public IActionResult NewJob([FromBody] string url)
        {
            this._logger.LogDebug($"VolumeDetect.Processing: {url}");

            BackgroundTask backgroundTask = new BackgroundTask(null, (task) =>
            {
                this._logger.LogDebug("Queued Background Task {Guid}", task.ID);
                try
                {
                    List<AudioMeter> volumeDetect = VolumeDetect.Process(url);

                    VolumeDetectDTO volumeDetectDTO = new VolumeDetectDTO()
                    {
                        url = url,
                        AudioMeters = volumeDetect.Select(item => item.ConvertToDTO()).ToList<AudioMeterDTO>(),
                    };
                    task.Result = volumeDetectDTO;
                } 
                finally
                {
                    this._logger.LogDebug("Finished Background Task {Guid}", task.ID);
                }
            });

            this._queue.Enqueue(backgroundTask);

            return Ok(
                backgroundTask.ConvertToDTO());
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BackgroundTaskDTO>), StatusCodes.Status200OK)]
        public IEnumerable<BackgroundTaskDTO> GetBackgroundTasks()
        {
            var tasks = this._queue.ToList();
            foreach (var task in tasks)
            {
                yield return task.ConvertToDTO();
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BackgroundTaskDTO), StatusCodes.Status200OK)]
        public IActionResult GetBackgroundTask(string id)
        {
            if (String.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid _id))
                return BadRequest("Task id is empty or bad");

            if (!this._queue.TryGet(Guid.Parse(id), out BackgroundTask? backgroundTask))
            {
                return NotFound();
            }

            return Ok(
                backgroundTask?.ConvertToDTO());
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BackgroundTaskDTO), StatusCodes.Status200OK)]
        public IActionResult DeleteBackgroundTask(string id)
        {
            if (String.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid _id))
                return BadRequest("Task id is empty or bad");

            if (!this._queue.TryDequeue(Guid.Parse(id), out BackgroundTask? backgroundTask))
            {
                return NotFound();
            }

            return Ok(
                backgroundTask?.ConvertToDTO());
        }


    }
}
