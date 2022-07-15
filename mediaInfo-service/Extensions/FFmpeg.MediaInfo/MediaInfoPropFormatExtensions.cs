using FFmpeg.MediaInfo.DTOs;
using FFmpeg.MediaInfo.Models;

namespace _MediaInfoService.Extensions
{
    public static class MediaInfoPropFormatExtensions
    {
        public static MediaInfoPropFormatDTO ConvertToDTO(this MediaInfoPropFormat format)
        {
            return new MediaInfoPropFormatDTO()
            {
                Name = format.Name,
                LongName = format.LongName
            };
        }
    }
}
