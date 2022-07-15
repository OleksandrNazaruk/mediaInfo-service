using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.DTOs
{
    public class MediaInfoPropAVRationalDTO
    {
        [JsonPropertyName("string")]
        public string? String { get; set; }

        [JsonPropertyName("num")]
        public int? num { get; set; }

        [JsonPropertyName("den")]
        public int? den { get; set; }

        public MediaInfoPropAVRationalDTO()
        {

        }
    }
}
