using System.Text.Json.Serialization;

namespace _MediaInfoService.Models
{
    public class BackgroundTaskDTO
    {
        [JsonPropertyName("id")]
        public Guid? ID { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("running_at")]
        public DateTime? RunningAt { get; set; }

        [JsonPropertyName("finished_at")]
        public DateTime? FinishedAt { get; set; }

        [JsonPropertyName("taskSettings")]
        public string? TaskSettings { get; set; }

        [JsonPropertyName("result")]
        public Object? Result { get; set; }

        public BackgroundTaskDTO()
        {

        }
    }
}
