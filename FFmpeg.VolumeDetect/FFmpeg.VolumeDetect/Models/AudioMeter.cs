using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.VolumeDetect.Models
{
    public class AudioMeter
    {
        public int? StreamIndex { get; set; }
        public ulong? Samples { get; set; }
        public double? MeanVolume { get; set; }
        public double? MaxVolume { get; set; }
        public IDictionary<string, UInt64> Histogram { get; set; }

        public AudioMeter(int streamIndex)
        {
            this.Histogram = new Dictionary<string, UInt64>();
            this.StreamIndex = streamIndex;
        }
    }
}
