using FFmpeg.MediaInfo.DTOs;
using FFmpeg.MediaInfo.Models;

namespace _MediaInfoService.Extensions
{
    public static class MediaInfoPropPairExtensions
    {
        public static MediaInfoPropPairDTO<TKey, TValue> ConvertToDTO<TKey, TValue>(this MediaInfoPropPair<TKey, TValue> pair)
        {
            return new MediaInfoPropPairDTO<TKey, TValue>()
            {
                String = pair.String,   
                Value = pair.Value
            };
        }
    }

}
