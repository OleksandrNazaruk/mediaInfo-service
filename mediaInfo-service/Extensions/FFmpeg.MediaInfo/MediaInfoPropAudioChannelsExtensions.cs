using FFmpeg.MediaInfo.DTOs;
using FFmpeg.MediaInfo.Models;

namespace _MediaInfoService.Extensions
{
    public static class MediaInfoPropAudioChannelsExtensions
    {
        public static MediaInfoPropAudioChannelsDTO ConvertToDTO(this MediaInfoPropAudioChannels audioChannels)
        {
            return new MediaInfoPropAudioChannelsDTO()
            {
                Layout = audioChannels.Layout,
                Value = audioChannels.Value,
                LayoutName = audioChannels.LayoutName,
            };
        }
    }
}
