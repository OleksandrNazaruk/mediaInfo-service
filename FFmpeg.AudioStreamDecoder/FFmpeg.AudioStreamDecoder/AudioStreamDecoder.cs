using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace FFmpeg.AudioStreamDecoder
{
    public sealed unsafe class AudioStreamDecoder : IDisposable
    {
        private readonly AVCodecContext* _pCodecContext = null;
        private readonly AVStream* _pStream = null;
        private readonly AVFrame* _pFrame = null;

        // SwrConvert
        AVSampleFormat _sampleFormat = AVSampleFormat.AV_SAMPLE_FMT_NONE;
        SwrContext* _pSwrContext = null;
        AVFrame* audioFrameConverted = null;
        public AVSampleFormat SampleFormat { get => (_sampleFormat == AVSampleFormat.AV_SAMPLE_FMT_NONE) ? _pCodecContext->sample_fmt : _sampleFormat; }


        public AVStream* Stream { get => _pStream; }
        public AVCodecContext* CodecContext { get => _pCodecContext; }

        /// <summary>
        /// 
        /// </summary>
        public int Index { get => _pStream->index; }

        /// <summary>
        /// 
        /// </summary>
        public string CodecName { get; }

        /// <summary>
        /// 
        /// </summary>
        public int Channels { get => _pCodecContext->channels; }

        /// <summary>
        /// 
        /// </summary>
        public int SampleRate { get => _pCodecContext->sample_rate; }


        public AudioStreamDecoder(AVStream* pStream, AVSampleFormat sampleFormat = AVSampleFormat.AV_SAMPLE_FMT_NONE)
        {
            _pStream = pStream;

            AVCodec* codec = null;
            codec = ffmpeg.avcodec_find_decoder(this._pStream->codecpar->codec_id);
            if (codec == null)
            {
                throw new ApplicationException("decoder not found");
            }

            _pCodecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (_pCodecContext == null)
            {
                throw new ApplicationException("alloc codec context error");
            }

            int ret = default;
            ret = ffmpeg.avcodec_parameters_to_context(_pCodecContext, this._pStream->codecpar);
            if (ret != 0)
                throw new ApplicationException(AudioDecoder.av_strerror(ret));

            ret = ffmpeg.avcodec_open2(_pCodecContext, codec, null);
            if (ret != 0)
                throw new ApplicationException(AudioDecoder.av_strerror(ret));

            this.CodecName = ffmpeg.avcodec_get_name(codec->id);

            _pFrame = ffmpeg.av_frame_alloc();


            // SwrConvert      
            if ((sampleFormat != AVSampleFormat.AV_SAMPLE_FMT_NONE) && (sampleFormat != _pCodecContext->sample_fmt))
            {
                this._sampleFormat = sampleFormat;
                _pSwrContext = ffmpeg.swr_alloc();
                ffmpeg.av_opt_set_int(_pSwrContext, "in_channel_layout", (int)_pCodecContext->channel_layout, 0);
                ffmpeg.av_opt_set_int(_pSwrContext, "out_channel_layout", (int)_pCodecContext->channel_layout, 0);
                ffmpeg.av_opt_set_int(_pSwrContext, "in_channel_count", _pCodecContext->channels, 0);
                ffmpeg.av_opt_set_int(_pSwrContext, "out_channel_count", _pCodecContext->channels, 0);
                ffmpeg.av_opt_set_int(_pSwrContext, "in_sample_rate", _pCodecContext->sample_rate, 0);
                ffmpeg.av_opt_set_int(_pSwrContext, "out_sample_rate", _pCodecContext->sample_rate, 0);
                ffmpeg.av_opt_set_sample_fmt(_pSwrContext, "in_sample_fmt", _pCodecContext->sample_fmt, 0);
                ffmpeg.av_opt_set_sample_fmt(_pSwrContext, "out_sample_fmt", _sampleFormat, 0);

                if (ffmpeg.swr_init(_pSwrContext) != 0)
                    throw new ApplicationException("Failed init SwrContext: ");

                audioFrameConverted = ffmpeg.av_frame_alloc();
                if (audioFrameConverted != null)
                {
                    audioFrameConverted->nb_samples = (_pCodecContext->frame_size <= 0) ? 32 : _pCodecContext->frame_size;
                    audioFrameConverted->format = (int)this.SampleFormat;
                    audioFrameConverted->channel_layout = _pCodecContext->channel_layout;
                    audioFrameConverted->channels = _pCodecContext->channels;
                    audioFrameConverted->sample_rate = _pCodecContext->sample_rate;
                }
            }

        }

        public Int16[] FrameToBuffer16(AVFrame* samples)
        {
            int nb_samples = samples->nb_samples;
            int nb_channels = samples->channels;
            int nb_planes = nb_channels;
            Int16* pcm;

            if (ffmpeg.av_sample_fmt_is_planar((AVSampleFormat)samples->format) <= 0)
            {
                nb_samples *= nb_channels;
                nb_planes = 1;
            }

            // size of sample
            int dataSize = ffmpeg.av_get_bytes_per_sample(_pCodecContext->sample_fmt);
            if (dataSize < 0)
                throw new Exception("Failed to calculate data size");

            Int16[] buffer = new Int16[nb_samples];


            for (int plane = 0; plane < nb_planes; plane++)
            {
                pcm = (Int16*)samples->extended_data[plane];
                for (int i = 0; i < nb_samples; i++)
                {
                    buffer[i] = pcm[i];
                }

            }
            return buffer;
        }

        public List<byte[]> FrameToBuffer(AVFrame* frame)
        {
            List<byte[]> channels = new List<byte[]>();

            bool isPlanar = ffmpeg.av_sample_fmt_is_planar((AVSampleFormat)frame->format) == 1;

            // size of sample
            int dataSize = ffmpeg.av_get_bytes_per_sample(_pCodecContext->sample_fmt);
            if (dataSize < 0)
                throw new Exception("Failed to calculate data size");

            // calculate one channel size
            int bufferSize = dataSize * frame->nb_samples;
            int channelCount = frame->channels;
            long channelLayout = unchecked((long)frame->channel_layout);
            AVSampleFormat format = (AVSampleFormat)frame->format;
            int samplesPerChannel = frame->nb_samples;
            int bufferLength = ffmpeg.av_samples_get_buffer_size(null, channelCount, samplesPerChannel, format, 1);

            for (int ch = 0; ch < _pCodecContext->channels; ch++)
                channels.Add(new byte[bufferSize]);

            // 
            for (int i = 0; i < frame->nb_samples; i++)
                for (int ch = 0; ch < _pCodecContext->channels; ch++)
                    Marshal.Copy((IntPtr)frame->data[(uint)ch] + dataSize * i, channels[ch], dataSize * i, dataSize);

            return channels;
        }

        /* read all the output frames (in general there may be any number of them */
        /**
         * https://blogs.gentoo.org/lu_zero/2016/03/29/new-avcodec-api/
         * You feed data using the avcodec_send_* functions until you get a AVERROR(EAGAIN), that signals that the internal input buffer is full.
         * You get the data back using the matching avcodec_receive_* function until you get a AVERROR(EAGAIN), signalling that the internal output buffer is empty.
         * Once you are done feeding data you have to pass a NULL to signal the end of stream.
         * You can keep calling the avcodec_receive_* function until you get AVERROR_EOF.
         * You free the contexts as usual.
        **/
        public void DecodePacket(AVPacket* pkt, AudioDecoder.ReceiveFrameCallBack OnReceiveFrame)
        {
            int ret = default;

            ffmpeg.av_frame_unref(_pFrame);

            /* send the packet with the compressed data to the decoder */
            ret = ffmpeg.avcodec_send_packet(_pCodecContext, pkt);
            if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN)) // Again EAGAIN is not expected
                return;
            else if (ret == ffmpeg.AVERROR_EOF)
                return;
            else if (ret < 0)
                throw new ApplicationException(AudioDecoder.av_strerror(ret)); //Failed to send the packet to the decoder

            /* read all the output frames (in general there may be any number of them */
            do
            {
                ret = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
                if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                    // The decoder doesn't have enough data to produce a frame
                    // Not an error unless we reached the end of the stream
                    // Just pass more packets until it has enough to produce a frame
                    return;
                else if (ret == ffmpeg.AVERROR_EOF)
                    return;
                else if (ret < 0)
                    // Failed to get a frame from the decoder
                    throw new ApplicationException(AudioDecoder.av_strerror(ret));

                // convert frame data to desired format
                if (_pSwrContext != null)
                {
                    byte* convertedData = null;
                    if (ffmpeg.av_samples_alloc(&convertedData, null, _pCodecContext->channels, audioFrameConverted->nb_samples, this._sampleFormat, 0) < 0)
                        throw new ApplicationException("failed samples memory alloc");
                    try
                    {
                        int samples = default;
                        fixed (byte** frame_data = (byte*[])_pFrame->data)
                        {
                            samples = ffmpeg.swr_convert(_pSwrContext, null, 0, frame_data, _pFrame->nb_samples);
                            if (samples < 0)
                                throw new ApplicationException("failed to convert frame");
                        }

                        for (; ; )
                        {
                            samples = ffmpeg.swr_get_out_samples(_pSwrContext, 0);
                            if ((samples < _pCodecContext->frame_size * _pCodecContext->channels) || _pCodecContext->frame_size == 0 && (samples < audioFrameConverted->nb_samples * _pCodecContext->channels))
                                break;

                            samples = ffmpeg.swr_convert(_pSwrContext, &convertedData, audioFrameConverted->nb_samples, null, 0);

                            int buffer_size = ffmpeg.av_samples_get_buffer_size(null, _pCodecContext->channels, audioFrameConverted->nb_samples, this._sampleFormat, 0);
                            if (buffer_size < 0)
                                throw new Exception("Invalid buffer size");

                            if (ffmpeg.avcodec_fill_audio_frame(audioFrameConverted, _pCodecContext->channels, this._sampleFormat, convertedData, buffer_size, 0) < 0)
                                throw new ApplicationException("Could not fill frame");

                            // frame ready
                            if (OnReceiveFrame != null)
                                OnReceiveFrame(audioFrameConverted, this._pStream->index);
                        }
                    }
                    finally
                    {
                        ffmpeg.av_freep(&convertedData);
                    }
                }
                else
                {
                    // frame ready
                    if (OnReceiveFrame != null)
                        OnReceiveFrame(this._pFrame, this._pStream->index);
                }

            } while (ret >= 0);
        }

        public void Dispose()
        {
            if (this._pSwrContext != null)
            {
                ffmpeg.swr_close(_pSwrContext);
                fixed (SwrContext** ptr = &_pSwrContext)
                    ffmpeg.swr_free(ptr);
            }
            if (audioFrameConverted != null)
            {
                ffmpeg.av_frame_unref(this.audioFrameConverted);
                ffmpeg.av_free(this.audioFrameConverted);
            }
            if (this._pCodecContext != null)
            {
                ffmpeg.avcodec_close(this._pCodecContext);
            }
            if (this._pCodecContext != null)
            {
                ffmpeg.av_frame_unref(this._pFrame);
                ffmpeg.av_free(this._pFrame);
            }
        }
    }
}
