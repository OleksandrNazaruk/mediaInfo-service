using FFmpeg.MediaInfo.DTOs;
using FFmpeg.MediaInfo.Models;

namespace _MediaInfoService.Extensions
{
    public static class MediaInfoPropAVRationalExensions
    {
        public static MediaInfoPropAVRationalDTO ConvertToDTO(this MediaInfoPropAVRational rational)
        {
            return new MediaInfoPropAVRationalDTO()
            {
                 den = rational.den,
                 num = rational.num,
                 String = rational.String,  
            };
        }
    }
}
