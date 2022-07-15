using FFmpeg.MediaInfo;
using FFmpeg.MediaInfo.DTOs;

namespace _MediaInfoService.Extensions
{
    public static class MediaInfoPropStreamExtensions
    {
        public static MediaInfoPropStreamDTO ConvertToDTO(this MediaInfoPropStream stream)
        {

            MediaInfoPropStreamDTO? result = default;

            if (stream is MediaInfoPropVideoStream)
            {
                result = ((MediaInfoPropVideoStream)stream).ConvertToDTO();
            }
            else
           if (stream is MediaInfoPropAudioStream)
            {
                result = ((MediaInfoPropAudioStream)stream).ConvertToDTO();
            }
            else
            {
                result = new MediaInfoPropStreamDTO()
                {
                    Codec = stream.Codec?.ConvertToDTO(),
                    Metadata = stream.Metadata,
                    StreamId = stream.StreamId,
                    StreamIndex = stream.StreamIndex,
                    StreamType = stream.StreamType,
                    TimeBase = stream.TimeBase?.ConvertToDTO(),
                    Type = stream.Type,
                };
            }

            return result;
        }
    }
}
