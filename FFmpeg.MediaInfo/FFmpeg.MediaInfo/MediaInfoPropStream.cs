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
    public unsafe class MediaInfoPropStream: IDisposable
    {
        internal readonly AVStream* _pAVStream = null;
        internal readonly AVFormatContext* _pAVFormatContext = null;
        internal readonly AVCodecContext* _pCodecContext = null;

        public int? StreamIndex { get; set; }
        public int? StreamId { get; set; }
        public string StreamType { get; set; }
        public string Type { get; set; }
        public MediaInfoPropCodec? Codec { get; set; }
        public MediaInfoPropAVRational? TimeBase { get; set; }
        public IDictionary<string, string>? Metadata { get; set; }

        private static string get_media_type_string(AVMediaType media_type)
        {
            switch (media_type)
            {
                case AVMediaType.AVMEDIA_TYPE_VIDEO: return "video";
                case AVMediaType.AVMEDIA_TYPE_AUDIO: return "audio";
                case AVMediaType.AVMEDIA_TYPE_DATA: return "data";
                case AVMediaType.AVMEDIA_TYPE_NB: return "nb";
                case AVMediaType.AVMEDIA_TYPE_SUBTITLE: return "subtitle";
                case AVMediaType.AVMEDIA_TYPE_ATTACHMENT: return "attachment";
                default: return "unknown";
            }
        }

        public void Dispose()
        {
            if (this._pCodecContext != null)
            {
                ffmpeg.avcodec_close(this._pCodecContext);
            }
        }

        public MediaInfoPropStream(int index, AVStream* AVStream, AVFormatContext* pFormatContext)
        {
            this._pAVStream = AVStream;
            this._pAVFormatContext = pFormatContext;


            AVCodec* codec = null;
            codec = ffmpeg.avcodec_find_decoder(this._pAVStream->codecpar->codec_id);
            if (codec == null)
            {
                throw new ApplicationException("decoder not found");
            }

            this._pCodecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (this._pCodecContext == null)
            {
                throw new ApplicationException("alloc codec context error");
            }

            int ret = default;
            ret = ffmpeg.avcodec_parameters_to_context(this._pCodecContext, this._pAVStream->codecpar);
            if (ret != 0)
                throw new ApplicationException(FFmpegHelper.av_strerror(ret));

            ret = ffmpeg.avcodec_open2(this._pCodecContext, codec, null);
            if (ret != 0)
                throw new ApplicationException(FFmpegHelper.av_strerror(ret));

     


            this.StreamIndex = this._pAVStream->index;
            this.StreamId = this._pAVStream->id;

            this.StreamType = ((AVMediaType)this._pAVStream->codecpar->codec_type).ToString();
            this.Metadata = MediaInfo.ToDictionary(this._pAVStream->metadata);

            // codec.name
            byte[] _buffer = new byte[255];
            var _buffer_string = "";
            fixed (byte* _pointer = &_buffer[0])
            {
                ffmpeg.avcodec_string(_pointer, _buffer.Length, this._pCodecContext, 0);
                _buffer_string = Marshal.PtrToStringAnsi((IntPtr)_pointer);
            }

            this.Codec = new MediaInfoPropCodec()
            {
                String = _buffer_string,
                Name = ffmpeg.avcodec_get_name(this._pAVStream->codecpar->codec_id),
                Profile = ffmpeg.avcodec_profile_name(this._pAVStream->codecpar->codec_id, this._pAVStream->codecpar->profile),
                Tag = new MediaInfoPropPair<long, string>()
                {
                    Value = this._pAVStream->codecpar->codec_tag,
                    String = MediaInfo.ToFourCCString(this._pAVStream->codecpar->codec_tag)
                }
            };

            // time_base
            this.TimeBase = new MediaInfoPropAVRational()
            {
                String = String.Format("{0}:{1}", this._pAVStream->time_base.num, this._pAVStream->time_base.den),
                num = this._pAVStream->time_base.num,
                den = this._pAVStream->time_base.den
            };

            // media_type
            //this.Type = ffmpeg.av_get_media_type_string((AVMediaType)pAVStream->codec->codec_type);
            this.Type = get_media_type_string(this._pAVStream->codecpar->codec_type);
        }


    }
}
