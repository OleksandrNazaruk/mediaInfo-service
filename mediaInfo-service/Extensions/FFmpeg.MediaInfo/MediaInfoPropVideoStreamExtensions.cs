using FFmpeg.MediaInfo;
using FFmpeg.MediaInfo.DTOs;

namespace _MediaInfoService.Extensions
{
    public static class MediaInfoPropVideoStreamExtensions
    {
        public static MediaInfoPropVideoStreamDTO ConvertToDTO(this MediaInfoPropVideoStream stream)
        {
            return new MediaInfoPropVideoStreamDTO()
            {
                // MediaInfoPropStream
                Codec = stream.Codec?.ConvertToDTO(),
                Metadata = stream.Metadata,
                StreamId = stream.StreamId,
                StreamIndex = stream.StreamIndex,
                StreamType = stream.StreamType,
                TimeBase = stream.TimeBase?.ConvertToDTO(),
                Type = stream.Type,

                // MediaInfoPropVideoStream
                Bitrate = stream.Bitrate?.ConvertToDTO(),
                RcMaxRate = stream.RcMaxRate?.ConvertToDTO(),
                BitsPerRawSample = stream.BitsPerRawSample?.ConvertToDTO(),
                HasBFrames = stream.HasBFrames,
                PixFmt = stream.PixFmt,
                Level = stream.Level,
                FieldOrder = stream.FieldOrder,
                ColorRange = stream.ColorRange,
                ChromaLocation = stream.ChromaLocation,
                ColorSpace = stream.ColorSpace,
                ColorTransfer = stream.ColorTransfer,
                ColorPrimaries = stream.ColorPrimaries,
                Width = stream.Width,
                Height = stream.Height,
                CodedWidth = stream.CodedWidth,
                CodedHeight = stream.CodedHeight,
                DisplayAspectRatio = stream.DisplayAspectRatio?.ConvertToDTO(),
                SampleAspectRatio = stream.SampleAspectRatio?.ConvertToDTO(),
                AvgFrameRate = stream.AvgFrameRate?.ConvertToDTO(),
                FPS = stream.FPS?.ConvertToDTO(),
                TBR = stream.TBR?.ConvertToDTO(),
                TBN = stream.TBN?.ConvertToDTO(),
                TBC = stream.TBC?.ConvertToDTO(),
            };
        }
    }
}
