using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace FFmpeg.AudioStreamDecoder
{
    public enum AVReturnCode
    {
        OK,
        ERROR,
        EOF
    }
    
    public sealed unsafe class AudioDecoder : IDisposable
    {
        public delegate void ReceiveFrameCallBack(AVFrame* frame, int streamIndex);

        private readonly AVFormatContext* _pFormatContext;
        private readonly AVPacket* _pPacket;

        private Dictionary<int, AudioStreamDecoder> streams = new Dictionary<int, AudioStreamDecoder>();

        public Dictionary<int, AudioStreamDecoder> Streams { get => this.streams; }

        public AudioDecoder(string url, AVSampleFormat sampleFormat = AVSampleFormat.AV_SAMPLE_FMT_NONE)
        {
            _pFormatContext = ffmpeg.avformat_alloc_context();
            var pFormatContext = _pFormatContext;

            int ret = default;
            ret = ffmpeg.avformat_open_input(&pFormatContext, url, null, null);
            if (ret != 0)
                throw new FileNotFoundException(AudioDecoder.av_strerror(ret), url);

            ret = ffmpeg.avformat_find_stream_info(_pFormatContext, null);
            if (ret != 0)
                throw new ApplicationException(AudioDecoder.av_strerror(ret));

            // search for the audio and video streams
            for (int i = 0; i < _pFormatContext->nb_streams; i++)
            {
                if (_pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    streams.Add(i, new AudioStreamDecoder(_pFormatContext->streams[i], sampleFormat));
                }
            }

            _pPacket = ffmpeg.av_packet_alloc();
        }


        public void Dispose()
        {
            ffmpeg.av_packet_unref(_pPacket);
            ffmpeg.av_free(_pPacket);

            // dispose streams
            System.Collections.IEnumerator enumerator = streams.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<int, AudioStreamDecoder> pair =
                    ((KeyValuePair<int, AudioStreamDecoder>)(enumerator.Current));
                //dispose it
                pair.Value.Dispose();
            }

            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
        }

        public static unsafe string av_strerror(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }

        private void FlushDecoders()
        {
            /* flush the decoder */
            var packet = new AVPacket();
            packet.data = null;
            packet.size = 0;
            foreach (KeyValuePair<int, AudioStreamDecoder> stream in this.streams)
            {
                stream.Value.DecodePacket(&packet, null);
            }
        }

        public AVReturnCode TryDecodeNextFrame(AudioDecoder.ReceiveFrameCallBack OnReceiveFrame)
        {
            int ret = default;
            AVReturnCode returnCode = AVReturnCode.OK;
            try
            {
                ret = ffmpeg.av_read_frame(_pFormatContext, _pPacket);
                if (ret == ffmpeg.AVERROR_EOF)
                    return AVReturnCode.EOF;
                else if (ret < 0)
                    throw new ApplicationException(AudioDecoder.av_strerror(ret));


                if (this.streams.TryGetValue(_pPacket->stream_index, out AudioStreamDecoder stream))
                {
                    stream.DecodePacket(_pPacket, OnReceiveFrame);
                    returnCode = AVReturnCode.OK;
                }
                else
                {
                    // skipped stream
                    returnCode = AVReturnCode.OK;
                }
            }
            catch (Exception e)
            {
                returnCode = AVReturnCode.ERROR;
                Console.WriteLine(e.Message);
            }
            finally
            {
                ffmpeg.av_packet_unref(_pPacket);
            }

            return returnCode;
        }

        public void Decode(AudioDecoder.ReceiveFrameCallBack OnReceiveFram = null)
        {
            AVReturnCode returnCode = default;
            do
            {
                returnCode = TryDecodeNextFrame(OnReceiveFram);

            } while (returnCode == AVReturnCode.OK);

            FlushDecoders();
        }

    }
}
