using FFmpeg.MediaInfo.DTOs;
using FFmpeg.MediaInfo.Models;

namespace _MediaInfoService.Extensions
{
    public static class MediaInfoPropCodecExtensions
    {
        public static MediaInfoPropCodecDTO ConvertToDTO(this MediaInfoPropCodec codec)
        {
            return new MediaInfoPropCodecDTO()
            {
                Name = codec.Name,
                Profile = codec.Profile,
                Tag = codec.Tag?.ConvertToDTO(),
                String = codec.String,
            };
        }
    }
}
