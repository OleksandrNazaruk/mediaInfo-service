using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.VolumeDetect.DTOs
{
    public class AudioMeterDTO
    {
        [JsonPropertyName("stream_index")]
        public int? StreamIndex { get; set; }

        [JsonPropertyName("nb_samples")]
        public ulong? Samples { get; set; }

        [JsonPropertyName("mean_volume_db")]
        public double? MeanVolume { get; set; }

        [JsonPropertyName("max_volume_db")]
        public double? MaxVolume { get; set; }

        [JsonPropertyName("histogram_db")]
        public IDictionary<string, UInt64>? Histogram { get; set; }

        public AudioMeterDTO()
        {

        }
    }
}
