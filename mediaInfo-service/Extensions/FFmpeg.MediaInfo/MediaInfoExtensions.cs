using FFmpeg.MediaInfo;
using FFmpeg.MediaInfo.DTOs;
using System.Linq;
using System;
using System.Collections.Generic;

namespace _MediaInfoService.Extensions
{
    public static class MediaInfoExtensions
    {
        public static MediaInfoDTO ConvertToDTO(this MediaInfo mediaInfo)
        {
            return new MediaInfoDTO()
            {
                Format = mediaInfo.Format?.ConvertToDTO(),
                VideoStreamIndex = mediaInfo.VideoStreamIndex,
                Metadata = mediaInfo.Metadata,
                Duration = new MediaInfoPropPairDTO<double, string>() 
                { 
                    Value = mediaInfo.Duration?.Value.TotalMilliseconds ?? 0,
                    String = mediaInfo.Duration?.String
                },
                StartTime = new MediaInfoPropPairDTO<double, string>()
                {
                    Value = mediaInfo.StartTime?.Value.TotalMilliseconds ?? 0,
                    String = mediaInfo.StartTime?.String
                },
                Bitrate = mediaInfo.Bitrate?.ConvertToDTO(),
                AVStreams = mediaInfo.AVStreams?.Select(item => item.ConvertToDTO()).ToList<object>(),
                Standard = mediaInfo.Standard,
                Filename = mediaInfo.Filename,
                Fullpath = mediaInfo.Fullpath,
            };
        }
    }

}
