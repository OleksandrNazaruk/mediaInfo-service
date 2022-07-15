using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.DTOs
{
    public class MediaInfoPropCodecDTO
    {
        [JsonPropertyName("string")]
        public string? String { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("profile")]
        public string? Profile { get; set; }

        [JsonPropertyName("tag")]
        public MediaInfoPropPairDTO<long, string>? Tag { get; set; }

        public MediaInfoPropCodecDTO()
        {

        }
    }
}
