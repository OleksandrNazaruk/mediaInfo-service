using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using FFmpeg.MediaInfo.Models;

namespace FFmpeg.MediaInfo
{
    public unsafe class MediaInfoPropVideoStream : MediaInfoPropStream
    {
        public MediaInfoPropPair<long, string>? Bitrate { get; set; }
        public MediaInfoPropPair<long, string>? RcMaxRate { get; set; }
        public MediaInfoPropPair<long, string>? BitsPerRawSample { get; set; }
        public bool? HasBFrames { get; set; }
        public string? PixFmt { get; set; }
        public int? Level { get; set; }
        public string? FieldOrder { get; set; }
        public string? ColorRange { get; set; }
        public string? ChromaLocation { get; set; }
        public string? ColorSpace { get; set; }
        public string? ColorTransfer { get; set; }
        public string? ColorPrimaries { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? CodedWidth { get; set; }
        public int? CodedHeight { get; set; }
        public MediaInfoPropAVRational? DisplayAspectRatio { get; set; }
        public MediaInfoPropAVRational? SampleAspectRatio { get; set; }
        public MediaInfoPropAVRational? AvgFrameRate { get; set; }
        public MediaInfoPropPair<double, string>? FPS { get; set; }
        public MediaInfoPropPair<double, string>? TBR { get; set; }
        public MediaInfoPropPair<double, string>? TBN { get; set; }
        public MediaInfoPropPair<double, string>? TBC { get; set; }

        public MediaInfoPropVideoStream(int index, AVStream* AVStream, AVFormatContext* pFormatContext) : base(index, AVStream, pFormatContext)
        {

            // bit_rate
            if (this._pAVStream->codecpar->bit_rate > 0)
            {
                this.Bitrate = new MediaInfoPropPair<long, string>()
                {
                    Value = this._pAVStream->codecpar->bit_rate,
                    String = String.Format("{0} Kbps", Convert.ToString(this._pAVStream->codecpar->bit_rate / 1000))
                };
            }

            // rc_max_rate
            if (this._pCodecContext->rc_max_rate > 0)
            {
                this.RcMaxRate = new MediaInfoPropPair<long, string>()
                {
                    Value = this._pCodecContext->rc_max_rate,
                    String = String.Format("{0} Kbps", Convert.ToString(this._pCodecContext->rc_max_rate / 1000))
                };
            }

            // bits_per_raw_sample
            if (this._pAVStream->codecpar->bits_per_raw_sample > 0)
            {
                this.BitsPerRawSample = new MediaInfoPropPair<long, string>()
                {
                    Value = this._pAVStream->codecpar->bits_per_raw_sample,
                    String = String.Format("{0} Kbps", Convert.ToString(this._pAVStream->codecpar->bits_per_raw_sample / 1000))
                };
            }

            // has_b_frames
            this.HasBFrames = (this._pAVStream->codecpar->video_delay > 0);

            // pix_fmt
            this.PixFmt = ffmpeg.av_get_pix_fmt_name((AVPixelFormat)this._pAVStream->codecpar->format);

            // level
            this.Level = this._pAVStream->codecpar->level;

            // field_order
            switch (this._pAVStream->codecpar->field_order)
            {
                case AVFieldOrder.AV_FIELD_PROGRESSIVE:
                    this.FieldOrder = "progressive";
                    break;
                case AVFieldOrder.AV_FIELD_BB:
                    this.FieldOrder = "bb";
                    break;
                case AVFieldOrder.AV_FIELD_BT:
                    this.FieldOrder = "bt";
                    break;
                case AVFieldOrder.AV_FIELD_TB:
                    this.FieldOrder = "tb";
                    break;
                case AVFieldOrder.AV_FIELD_TT:
                    this.FieldOrder = "tt";
                    break;
                default:
                    this.FieldOrder = "unknown";
                    break;
            }

            // color_range
            if (this._pAVStream->codecpar->color_range != AVColorRange.AVCOL_RANGE_UNSPECIFIED)
                this.ColorRange = ffmpeg.av_color_range_name(this._pAVStream->codecpar->color_range);

            // chroma_sample_location
            if (this._pCodecContext->chroma_sample_location != AVChromaLocation.AVCHROMA_LOC_UNSPECIFIED)
                this.ChromaLocation = ffmpeg.av_chroma_location_name(this._pAVStream->codecpar->chroma_location);

            // color_space
            this.ColorSpace = ffmpeg.av_color_space_name(this._pAVStream->codecpar->color_space);

            // color_trc
            this.ColorTransfer = ffmpeg.av_color_transfer_name(this._pAVStream->codecpar->color_trc);

            // color_primaries
            this.ColorPrimaries = ffmpeg.av_color_primaries_name(this._pAVStream->codecpar->color_primaries);

            // width and height
            if (this._pAVStream->codecpar->width > 0)
            {
                this.Width = this._pAVStream->codecpar->width;
                this.Height = this._pAVStream->codecpar->height;
                if (this._pCodecContext->coded_width > 0 & this._pCodecContext->coded_height > 0 & ((this._pAVStream->codecpar->width != this._pCodecContext->coded_width) || (this._pAVStream->codecpar->height != this._pCodecContext->coded_height)))
                {
                    this.CodedWidth = this._pCodecContext->coded_width;
                    this.CodedHeight = this._pCodecContext->coded_height;
                }
            }

            // display_aspect_ratio, sample_aspect_ratio
            var _sar = ffmpeg.av_guess_sample_aspect_ratio(this._pAVFormatContext, this._pAVStream, null);
            if (_sar.num > 0)
            {
                // sample_aspect_ratio
                this.SampleAspectRatio = new MediaInfoPropAVRational()
                {
                    String = String.Format("{0}:{1}", _sar.num, _sar.den),
                    den = _sar.den,
                    num = _sar.num
                };

                AVRational _dar = new AVRational();
                ffmpeg.av_reduce(&_dar.num, &_dar.den, this._pAVStream->codecpar->width * _sar.num, this._pAVStream->codecpar->height * _sar.den, 1024 * 1024);
                // display_aspect_ratio
                this.DisplayAspectRatio = new MediaInfoPropAVRational()
                {
                    String = String.Format("{0}:{1}", _dar.num, _dar.den), //MediaInfo.ToAspectRatioString(_dar.num, _dar.den),
                    den = _dar.den,
                    num = _dar.num
                };

            }

            /**
               // display_aspect_ratio, sample_aspect_ratio
               if (pAVStream->codec->sample_aspect_ratio.num > 0)
               {
                   AVRational display_aspect_ratio = new AVRational();
                   ffmpeg.av_reduce(&display_aspect_ratio.num, &display_aspect_ratio.den, pAVStream->codec->width * pAVStream->codec->sample_aspect_ratio.num, pAVStream->codec->height * pAVStream->codec->sample_aspect_ratio.den, 1024 * 1024);

                   // display_aspect_ratio
                   this.DisplayAspectRatio = new MediaInfoProp_AVRational()
                   {
                       String = String.Format("{0}:{1}", display_aspect_ratio.num, display_aspect_ratio.den),
                       den = display_aspect_ratio.den,
                       num = display_aspect_ratio.num
                   };
                   // sample_aspect_ratio
                   this.SampleAspectRatio = new MediaInfoProp_AVRational()
                   {
                       String = String.Format("{0}:{1}", pAVStream->codec->sample_aspect_ratio.num, pAVStream->codec->sample_aspect_ratio.den),
                       den = pAVStream->codec->sample_aspect_ratio.den,
                       num = pAVStream->codec->sample_aspect_ratio.num
                   };
               }
            **/


            // avg_frame_rate
            this.AvgFrameRate = new MediaInfoPropAVRational()
            {
                String = String.Format("{0}:{1}", this._pAVStream->avg_frame_rate.num, this._pAVStream->avg_frame_rate.den),
                den = this._pAVStream->avg_frame_rate.den,
                num = this._pAVStream->avg_frame_rate.num
            };

            // fps
            var _fps = this._pAVStream->avg_frame_rate.den & this._pAVStream->avg_frame_rate.num;
            if (_fps > 0)
            {
                double ration = ffmpeg.av_q2d(this._pAVStream->avg_frame_rate);

                this.FPS = new MediaInfoPropPair<double, string>()
                {
                    Value = ration,
                    String = String.Format("{0:0.0000}", ration)
                };
            }

            // tbr
            var _tbr = this._pAVStream->r_frame_rate.den & this._pAVStream->r_frame_rate.num;
            if (_tbr > 0)
            {
                double ration = ffmpeg.av_q2d(this._pAVStream->r_frame_rate);
                this.TBR = new MediaInfoPropPair<double, string>()
                {
                    Value = ration,
                    String = String.Format("{0:0.0000}", ration)
                };
            }

            // tbn
            var _tbn = this._pAVStream->time_base.den & this._pAVStream->time_base.num;
            if (_tbn > 0)
            {
                double ration = 1 / ffmpeg.av_q2d(this._pAVStream->time_base);
                this.TBN = new MediaInfoPropPair<double, string>()
                {
                    Value = ration,
                    String = String.Format("{0:0.0000}", ration)
                };
            }

            // tbr
            var _tbc = this._pCodecContext->time_base.den & this._pCodecContext->time_base.num;
            if (_tbc > 0)
            {
                double ration = 1 / ffmpeg.av_q2d(this._pCodecContext->time_base);
                this.TBC = new MediaInfoPropPair<double, string>()
                {
                    Value = ration,
                    String = String.Format("{0:0.0000}", ration)
                };
            }
        }


    }
}
