using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.DTOs
{
    public class MediaInfoPropStreamDTO
    {
        [JsonPropertyName("stream_index")]
        public int? StreamIndex { get; set; }

        [JsonPropertyName("stream_id")]
        public int? StreamId { get; set; }

        [JsonPropertyName("stream_type")]
        public string? StreamType { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("codec")]
        public MediaInfoPropCodecDTO? Codec { get; set; }

        [JsonPropertyName("time_base")]
        public MediaInfoPropAVRationalDTO? TimeBase { get; set; }

        [JsonPropertyName("metadata")]
        public IDictionary<string, string>? Metadata { get; set; }

        public MediaInfoPropStreamDTO()
        {

        }
    }
}
