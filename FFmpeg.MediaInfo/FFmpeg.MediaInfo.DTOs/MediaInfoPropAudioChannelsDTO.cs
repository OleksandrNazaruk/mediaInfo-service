using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.DTOs
{
    public class MediaInfoPropAudioChannelsDTO
    {
        [JsonPropertyName("value")]
        public int? Value { get; set; }

        [JsonPropertyName("layout")]
        public ulong? Layout { get; set; }

        [JsonPropertyName("layout_name")]
        public string? LayoutName { get; set; }

        public MediaInfoPropAudioChannelsDTO()
        {
                
        }

    }

}
