using System.Text.Json.Serialization;

namespace FFmpeg.MediaInfo.DTOs
{
    public class MediaInfoDTO
    {
        [JsonPropertyName("format")]
        public MediaInfoPropFormatDTO? Format { get; set; }

        [JsonPropertyName("video_stream_index")]
        public int? VideoStreamIndex { get; set; }

        [JsonPropertyName("metadata")]
        public IDictionary<string, string>? Metadata { get; set; }

        [JsonPropertyName("duration")]
        public MediaInfoPropPairDTO<double, string>? Duration { get; set; }

        [JsonPropertyName("start_time")]
        public MediaInfoPropPairDTO<double, string>? StartTime { get; set; }

        [JsonPropertyName("bitrate")]
        public MediaInfoPropPairDTO<long, string>? Bitrate { get; set; }

        [JsonPropertyName("streams")]
        public IList<object>? AVStreams { get; set; }

        [JsonPropertyName("standard")]
        public string? Standard { get; set; } = String.Empty;

        [JsonPropertyName("filename")]
        public string? Filename { get; set; } = String.Empty;

        [JsonPropertyName("fullpath")]
        public string? Fullpath { get; set; } = String.Empty;

        public MediaInfoDTO()
        {

        }
    }
}