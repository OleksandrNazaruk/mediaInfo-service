using FFmpeg.VolumeDetect.DTOs;
using FFmpeg.VolumeDetect.Models;

namespace _MediaInfoService.Extensions
{
    public static class AudioMeterExtensions
    {
        public static AudioMeterDTO ConvertToDTO(this AudioMeter audioMeter)
        {
            return new AudioMeterDTO()
            {
                StreamIndex = audioMeter.StreamIndex,
                Histogram = audioMeter.Histogram,
                MaxVolume = audioMeter.MaxVolume,
                MeanVolume = audioMeter.MeanVolume,
                Samples = audioMeter.Samples,
            };
        }
    }
}
