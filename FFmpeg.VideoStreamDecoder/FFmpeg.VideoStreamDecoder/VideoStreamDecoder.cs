using FFmpeg.AutoGen;
using System.Runtime.InteropServices;

namespace FFmpeg.VideoStreamDecoder
{
    public static class VideoStreamDecoder
    {
        public static unsafe string GetErrorMessage(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringUTF8((IntPtr)buffer);
            return message ?? String.Empty;
        }

        public static unsafe SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgr24> GetThumbnail(string url, int frameIndex, int width, int height)
        {
            int ret;

            AVFrame* receivedFrame = ffmpeg.av_frame_alloc();

            // Allocate a file format context to the file we will be streaming and make sure we can open it.
            AVFormatContext* pFormatContext = ffmpeg.avformat_alloc_context();
            try
            {
                ret = ffmpeg.avformat_open_input(&pFormatContext, url, null, null);
                if (ret < 0)
                    throw new FileNotFoundException(VideoStreamDecoder.GetErrorMessage(ret), url);

                // If we could open it, make sure we are able to obtain stream information from the file.
                ret = ffmpeg.avformat_find_stream_info(pFormatContext, null);
                if (ret < 0)
                    throw new ApplicationException(VideoStreamDecoder.GetErrorMessage(ret));

                /*
                            // Set up a video stream context if there is a video stream in the file.
                            AVStream* videoStream = null;
                            for (var i = 0; i < pFormatContext->nb_streams; i++)
                            {
                                if (pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                                {
                                    Console.WriteLine(@"We found a potential video stream.");
                                    videoStream = pFormatContext->streams[i];
                                    break;
                                }
                            }
                            // Make sure there is a video stream in the AVStream variable we just set.
                            if (videoStream == null)
                            {
                                throw new ApplicationException(@"We could not find video stream from the file context.");
                            }
                */

                AVCodec* codec = null;
                int streamIndex = ffmpeg.av_find_best_stream(pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0);
                if (streamIndex < 0)
                {
                    throw new ApplicationException(@"We could not find video stream from the file context.");
                }

                if (codec == null)
                {
                    throw new ApplicationException(@"We could not find the codec specified; it could be unsupported.");
                }

                //
                AVCodecContext* pCodecContext = ffmpeg.avcodec_alloc_context3(codec);
                if (pCodecContext == null)
                {
                    throw new ApplicationException("alloc codec context error");
                }
                try
                {
                    //
                    ret = ffmpeg.avcodec_parameters_to_context(pCodecContext, pFormatContext->streams[streamIndex]->codecpar);
                    if (ret < 0)
                        throw new ApplicationException(VideoStreamDecoder.GetErrorMessage(ret));


                    // Check the decoder's codec capabilities and if it is equal to truncated, set the decoding codec context flag to truncated.
                    if ((codec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
                    {
                        pCodecContext->flags |= ffmpeg.AV_CODEC_CAP_TRUNCATED;
                    }


                    // Make sure we can open the context to decode.
                    ret = ffmpeg.avcodec_open2(pCodecContext, codec, null);
                    if (ret < 0)
                        throw new ApplicationException(VideoStreamDecoder.GetErrorMessage(ret));


                    // Set up the conversion context.
                    int dstWidth = width == 0 ? pCodecContext->width : width;
                    int dstHeight = height == 0 ? pCodecContext->height : height;
                    SwsContext* pConvertContext = ffmpeg.sws_getContext(pCodecContext->width, pCodecContext->height, pCodecContext->pix_fmt, dstWidth, dstHeight, AVPixelFormat.AV_PIX_FMT_BGR24, ffmpeg.SWS_FAST_BILINEAR, null, null, null);
                    if (pConvertContext == null)
                        throw new ApplicationException("Could not initialize the conversion context.");
                    try
                    {
                        // Allocate our converted frame and buffer and fill the picture.
                        int convertedFrameBufferSize = ffmpeg.av_image_get_buffer_size(AVPixelFormat.AV_PIX_FMT_BGR24, dstWidth, dstHeight, 1);
                        IntPtr convertedFrameBufferPtr = Marshal.AllocHGlobal(convertedFrameBufferSize);
                        try
                        {
                            byte_ptrArray4 dstData = new byte_ptrArray4();
                            int_array4 dstLinesize = new int_array4();
                            ffmpeg.av_image_fill_arrays(ref dstData, ref dstLinesize, (byte*)convertedFrameBufferPtr, AVPixelFormat.AV_PIX_FMT_BGR24, dstWidth, dstHeight, 1);



                            /* this.CodecName = ffmpeg.avcodec_get_name(codec->id);
                             this.FrameSize = new Size(this._pCodecContext->width, _pCodecContext->height);
                             this.PixelFormat = _pCodecContext->pix_fmt;


                             this._
                             this._
                             this._avgFrameDuration = TimeSpan.TicksPerSecond / ffmpeg.av_q2d(_pFormatContext->streams[_streamIndex]->avg_frame_rate); // eg. 1 sec / 25 fps = 400.000 ticks (40ms)
                            */

                            // seek
                            // Convert timebase to ticks so we can easily convert stream's timestamps to ticks
                            double streamTimebase = ffmpeg.av_q2d(pFormatContext->streams[streamIndex]->time_base) * TimeSpan.TicksPerSecond;
                            // We will need this when we seek (adding it to seek timestamp)
                            long startTime = pFormatContext->streams[streamIndex]->start_time != ffmpeg.AV_NOPTS_VALUE
                                ? (long)(pFormatContext->streams[streamIndex]->start_time * streamTimebase)
                                : 0;
                            //long seekTarget = (long)(TimeSpan.FromSeconds(1).Ticks / streamTimebase);
                            var pts = (Math.Max(0, frameIndex) * pFormatContext->streams[streamIndex]->r_frame_rate.den * pFormatContext->streams[streamIndex]->time_base.den) / (pFormatContext->streams[streamIndex]->r_frame_rate.num * pFormatContext->streams[streamIndex]->time_base.num);
                            // Seeking at frameTimestamp or previous I/Key frame and flushing codec
                            ret = ffmpeg.av_seek_frame(pFormatContext, streamIndex, startTime + pts, ffmpeg.AVSEEK_FLAG_BACKWARD);
                            ffmpeg.avcodec_flush_buffers(pCodecContext);


                            // read packet and decode
                            AVPacket* pPacket = ffmpeg.av_packet_alloc();
                            AVFrame* pFrame = ffmpeg.av_frame_alloc();
                            try
                            {
                                do
                                {
                                    try
                                    {
                                        do
                                        {
                                            ffmpeg.av_packet_unref(pPacket);
                                            ret = ffmpeg.av_read_frame(pFormatContext, pPacket);

                                            if (ret == ffmpeg.AVERROR_EOF)
                                                throw new ApplicationException(VideoStreamDecoder.GetErrorMessage(ret));

                                            if (ret < 0)
                                                throw new ApplicationException(VideoStreamDecoder.GetErrorMessage(ret));

                                        } while (pPacket->stream_index != streamIndex);

                                        ret = ffmpeg.avcodec_send_packet(pCodecContext, pPacket);
                                        if (ret < 0)
                                            throw new ApplicationException(VideoStreamDecoder.GetErrorMessage(ret));
                                    }
                                    finally
                                    {
                                        ffmpeg.av_packet_unref(pPacket);
                                    }

                                    ret = ffmpeg.avcodec_receive_frame(pCodecContext, pFrame);
                                } while (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN));

                                if (ret < 0)
                                    throw new ApplicationException(VideoStreamDecoder.GetErrorMessage(ret));

                                // convert
                                AVFrame sourceFrame = *pFrame;
                                ffmpeg.sws_scale(pConvertContext, sourceFrame.data, sourceFrame.linesize, 0, sourceFrame.height, dstData, dstLinesize);

                                var data = new byte_ptrArray8();
                                data.UpdateFrom(dstData);
                                var linesize = new int_array8();
                                linesize.UpdateFrom(dstLinesize);

                                AVFrame convertedFrame = new AVFrame
                                {
                                    data = data,
                                    linesize = linesize,
                                    width = dstWidth,
                                    height = dstHeight
                                };

                                var size = convertedFrame.height * convertedFrame.linesize[0];
                                var managedArray = new byte[size];
                                unsafe { Marshal.Copy((IntPtr)convertedFrame.data[0], managedArray, 0, size); }
                                return SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Bgr24>(managedArray, convertedFrame.width, convertedFrame.height);

                                /* 
                                  using (Bitmap bitmap = new Bitmap(convertedFrame.width, convertedFrame.height, convertedFrame.linesize[0], PixelFormat.Format24bppRgb, (IntPtr)convertedFrame.data[0]))
                                  {
                                      bitmap.Save($"frame.test.jpg", ImageFormat.Jpeg);
                                  }
                                */

                            }
                            finally
                            {
                                var _pFrame = pFrame;
                                ffmpeg.av_frame_free(&_pFrame);

                                var _pPacket = pPacket;
                                ffmpeg.av_packet_free(&_pPacket);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(convertedFrameBufferPtr);
                        }
                    }
                    finally
                    {
                        ffmpeg.sws_freeContext(pConvertContext);
                    }
                }
                finally
                {
                    ffmpeg.avcodec_close(pCodecContext);
                }
            }
            finally
            {
                var _pFormatContext = pFormatContext;
                ffmpeg.avformat_close_input(&_pFormatContext);
            }
        }
    }
}
