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
    public unsafe class MediaInfoPropAudioStream : MediaInfoPropStream
    {

        public MediaInfoPropPair<long, string>? Bitrate { get; set; }
        public MediaInfoPropAudioChannels? Channels { get; set; }
        public int? SampleRate { get; set; }
        public string SampleFmt { get; set; }
        public int? BitsPerSample { get; set; }
        public MediaInfoPropAudioStream(int index, AVStream* AVStream, AVFormatContext* pFormatContext) : base(index, AVStream, pFormatContext)
        {

            // sample_rate
            this.SampleRate = this._pAVStream->codecpar->sample_rate;

            // bits_per_sample
            var bits_per_sample = ffmpeg.av_get_bits_per_sample(this._pAVStream->codecpar->codec_id);
    
            // sample_fmt
            this.SampleFmt = ffmpeg.av_get_sample_fmt_name((AVSampleFormat)this._pAVStream->codecpar->format);


            // channels
            var channel_layout_name = "unknown";
            byte[] _buffer = new byte[255];
            fixed (byte* _pointer = &_buffer[0])
            {
                ffmpeg.av_get_channel_layout_string(_pointer, _buffer.Length, this._pAVStream->codecpar->channels, this._pAVStream->codecpar->channel_layout);
                channel_layout_name = Marshal.PtrToStringAnsi((IntPtr)_pointer);
            }
            this.Channels = new MediaInfoPropAudioChannels()
            {
                Value = this._pAVStream->codecpar->channels,
                Layout = this._pAVStream->codecpar->channel_layout,
                LayoutName = channel_layout_name
            };

            // bit_rate
            long bit_rate = (bits_per_sample > 0) ? this._pAVStream->codecpar->sample_rate * this._pAVStream->codecpar->channels * bits_per_sample : this._pAVStream->codecpar->bit_rate;
            if (bit_rate > 0)
            {
                this.Bitrate = new MediaInfoPropPair<long, string>()
                {
                    Value = bit_rate,
                    String = String.Format("{0} Kbps", Convert.ToString(bit_rate / 1000))
                };
            }

            // bits_per_sample
            if ((this._pAVStream->codecpar->bits_per_raw_sample > 0) && (this._pAVStream->codecpar->bits_per_raw_sample != ffmpeg.av_get_bytes_per_sample((AVSampleFormat)this._pAVStream->codecpar->format) * 8))
                this.BitsPerSample = this._pAVStream->codecpar->bits_per_raw_sample;
            else if (bits_per_sample > 0)
                this.BitsPerSample = bits_per_sample;
        }



    }
}
