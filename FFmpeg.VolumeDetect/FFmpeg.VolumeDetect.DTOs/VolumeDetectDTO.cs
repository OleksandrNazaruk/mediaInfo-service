using System.Text.Json.Serialization;

namespace FFmpeg.VolumeDetect.DTOs
{
    public class VolumeDetectDTO
    {
        [JsonPropertyName("url")]
        public string? url { get; set; }

        [JsonPropertyName("channels")]
        public List<AudioMeterDTO>? AudioMeters { get; set; }

        public VolumeDetectDTO()
        {

        }
    }
}