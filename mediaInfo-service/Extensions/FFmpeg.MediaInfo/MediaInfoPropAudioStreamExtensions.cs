using FFmpeg.MediaInfo;
using FFmpeg.MediaInfo.DTOs;

namespace _MediaInfoService.Extensions
{
    public static class MediaInfoPropAudioStreamExtensions
    {
        public static MediaInfoPropAudioStreamDTO ConvertToDTO(this MediaInfoPropAudioStream stream)
        {
            return new MediaInfoPropAudioStreamDTO()
            {
                // MediaInfoPropStream
                Codec = stream.Codec?.ConvertToDTO(),
                Metadata = stream.Metadata,
                StreamId = stream.StreamId,
                StreamIndex = stream.StreamIndex,
                StreamType = stream.StreamType,
                TimeBase = stream.TimeBase?.ConvertToDTO(),
                Type = stream.Type,

                // MediaInfoPropAudioStream
                Bitrate = stream.Bitrate?.ConvertToDTO(),
                Channels = stream.Channels?.ConvertToDTO(),
                SampleRate = stream.SampleRate,
                SampleFmt = stream.SampleFmt,
                BitsPerSample = stream.BitsPerSample,

            };
        }
    }
}
