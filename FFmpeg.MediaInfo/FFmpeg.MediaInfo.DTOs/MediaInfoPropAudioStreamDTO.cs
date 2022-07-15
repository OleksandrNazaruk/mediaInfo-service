using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.DTOs
{
    public class MediaInfoPropAudioStreamDTO: MediaInfoPropStreamDTO
    {
        [JsonPropertyName("bitrate")]
        public MediaInfoPropPairDTO<long, string>? Bitrate { get; set; }

        [JsonPropertyName("channels")]
        public MediaInfoPropAudioChannelsDTO? Channels { get; set; }

        [JsonPropertyName("sample_rate")]
        public int? SampleRate { get; set; }

        [JsonPropertyName("sample_fmt")]
        public string? SampleFmt { get; set; }

        [JsonPropertyName("bits_per_sample")]
        public int? BitsPerSample { get; set; }

        public MediaInfoPropAudioStreamDTO()
        {

        }
    }
}
