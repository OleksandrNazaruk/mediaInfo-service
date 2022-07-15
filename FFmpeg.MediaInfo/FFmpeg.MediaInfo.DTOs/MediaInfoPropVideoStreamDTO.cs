using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.DTOs
{
    public class MediaInfoPropVideoStreamDTO: MediaInfoPropStreamDTO
    {
        [JsonPropertyName("bitrate")]
        public MediaInfoPropPairDTO<long, string>? Bitrate { get; set; }

        [JsonPropertyName("rc_max_rate")]
        public MediaInfoPropPairDTO<long, string>? RcMaxRate { get; set; }

        [JsonPropertyName("bits_per_raw_sample")]
        public MediaInfoPropPairDTO<long, string>? BitsPerRawSample { get; set; }

        [JsonPropertyName("has_b_frames")]
        public bool? HasBFrames { get; set; }

        [JsonPropertyName("pix_fmt")]
        public string? PixFmt { get; set; }

        [JsonPropertyName("level")]
        public int? Level { get; set; }

        [JsonPropertyName("field_order")]
        public string? FieldOrder { get; set; }

        [JsonPropertyName("color_range")]
        public string? ColorRange { get; set; }

        [JsonPropertyName("chroma_location")]
        public string? ChromaLocation { get; set; }

        [JsonPropertyName("color_space")]
        public string? ColorSpace { get; set; }

        [JsonPropertyName("color_trc")]
        public string? ColorTransfer { get; set; }

        [JsonPropertyName("color_primaries")]
        public string? ColorPrimaries { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("coded_width")]
        public int? CodedWidth { get; set; }

        [JsonPropertyName("coded_height")]
        public int? CodedHeight { get; set; }

        [JsonPropertyName("display_aspect_ratio")]
        public MediaInfoPropAVRationalDTO? DisplayAspectRatio { get; set; }

        [JsonPropertyName("sample_aspect_ratio")]
        public MediaInfoPropAVRationalDTO? SampleAspectRatio { get; set; }

        [JsonPropertyName("avg_frame_rate")]
        public MediaInfoPropAVRationalDTO? AvgFrameRate { get; set; }

        [JsonPropertyName("fps")]
        public MediaInfoPropPairDTO<double, string>? FPS { get; set; }

        [JsonPropertyName("tbr")]
        public MediaInfoPropPairDTO<double, string>? TBR { get; set; }

        [JsonPropertyName("tbn")]
        public MediaInfoPropPairDTO<double, string>? TBN { get; set; }

        [JsonPropertyName("tbc")]
        public MediaInfoPropPairDTO<double, string>? TBC { get; set; }

        public MediaInfoPropVideoStreamDTO()
        {

        }
    }
}
