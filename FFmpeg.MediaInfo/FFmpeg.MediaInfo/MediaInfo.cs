/**
    MediaInfo mediaInfo = new MediaInfo(@"\\172.16.200.222\Public\MTU0070836_TEST.mov");
    Console.WriteLine((string)JsonSerializer.Serialize<MediaInfo>(mediaInfo, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        IgnoreNullValues = true,
    }));

**/

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
    public unsafe class MediaInfo: IDisposable
    {
        public MediaInfoPropFormat? Format { get; set; }
        public int? VideoStreamIndex { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public MediaInfoPropPair<TimeSpan, string>? Duration { get; set; }
        public MediaInfoPropPair<TimeSpan, string>? StartTime { get; set; }
        public MediaInfoPropPair<long, string>? Bitrate { get; set; }
        public List<MediaInfoPropStream>? AVStreams { get; set; }
        public string? Standard { get; set; }
        public string? Filename { get; set; }
        public string? Fullpath { get; set; }

        public MediaInfo()
        {

        }
        public MediaInfo(string url)
        {
            this.Probe(url);
        }

        public void Dispose()
        {
            foreach (MediaInfoPropStream AVStream in this.AVStreams)
            {
                AVStream.Dispose();
            }
        }

        public unsafe void Probe(string url)
        {
            AVFormatContext* pFormatContext = ffmpeg.avformat_alloc_context();
            try
            {
                // Filename
                this.Filename = System.IO.Path.GetFileName(url);
                // Fullpath
                this.Fullpath = System.IO.Path.GetDirectoryName(url);

                int ret = default;
                ret = ffmpeg.avformat_open_input(&pFormatContext, url, null, null);
                if (ret != 0)
                    throw new FileNotFoundException(MediaInfo.av_strerror(ret), url);
                try
                {
                    ret = ffmpeg.avformat_find_stream_info(pFormatContext, null);
                    if (ret != 0)
                        throw new ApplicationException(MediaInfo.av_strerror(ret));

                    // Print detailed information about the input or output format, such as duration, bitrate, streams, container, programs, metadata, side data, codec and time base. 
                    //ffmpeg.av_dump_format(pFormatContext, 0, url, 0);

                    // Find the best stream in the file. The best stream is determined according to various heuristics as the most likely to be what the user expects. 
                    this.VideoStreamIndex = ffmpeg.av_find_best_stream(pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, null, 0);

                    // format
                    this.Format = new MediaInfoPropFormat()
                    {
                        Name = Marshal.PtrToStringAnsi((IntPtr)pFormatContext->iformat->name),
                        LongName = Marshal.PtrToStringAnsi((IntPtr)pFormatContext->iformat->long_name),

                    };

                    // metadata
                    this.Metadata = ToDictionary(pFormatContext->metadata);

                    // duration
                    if (pFormatContext->duration != ffmpeg.AV_NOPTS_VALUE)
                    {
                        TimeSpan _timeSpan = ToTimeSpan(pFormatContext->duration);
                        this.Duration = new MediaInfoPropPair<TimeSpan, string>()
                        {
                            Value = _timeSpan,
                            String = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", _timeSpan.Hours, _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds)
                        };
                    }

                    // start_time
                    if (pFormatContext->start_time != ffmpeg.AV_NOPTS_VALUE)
                    {
                        //var _startTime = pFormatContext->start_time;
                        //var _sec = _startTime / ffmpeg.AV_TIME_BASE;
                        //var _ms = Math.Abs(_startTime % ffmpeg.AV_TIME_BASE);
                        TimeSpan _startTime = ToTimeSpan(pFormatContext->start_time);
                        this.StartTime = new MediaInfoPropPair<TimeSpan, string>()
                        {
                            Value = _startTime,
                            String = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", _startTime.Hours, _startTime.Minutes, _startTime.Seconds, _startTime.Milliseconds)
                            // String = String.Format("{0}.{1:000000}", _sec, ffmpeg.av_rescale(_ms, 1000000, ffmpeg.AV_TIME_BASE))
                        };
                    }

                    // bit_rate
                    if (pFormatContext->bit_rate != ffmpeg.AV_NOPTS_VALUE)
                    {
                        var _bitRate = pFormatContext->bit_rate;
                        if (_bitRate > 0)
                        {
                            this.Bitrate = new MediaInfoPropPair<long, string>()
                            {
                                Value = pFormatContext->bit_rate,
                                String = String.Format("{0} Kbps", (pFormatContext->bit_rate / 1000)),
                            };
                        }
                    }

                    // Collect information about streams
                    this.AVStreams = new List<MediaInfoPropStream>();
                    for (int i = 0; i < pFormatContext->nb_streams; i++)
                    {
                        AVStream* pAVStream = pFormatContext->streams[i];
                        if (pAVStream->codecpar->codec_id == AVCodecID.AV_CODEC_ID_PROBE)
                        {
                            // Failed to probe codec for input stream
                            continue;
                        }

                        switch (pAVStream->codecpar->codec_type)
                        {
                            case AVMediaType.AVMEDIA_TYPE_VIDEO:
                                var avstream = new MediaInfoPropVideoStream(i, pAVStream, pFormatContext);
                                this.AVStreams.Add(avstream);

                                if (this.VideoStreamIndex == i)
                                    this.Standard = GetStandard((int)avstream.Width, (int)avstream.Height);

                                break;
                            case AVMediaType.AVMEDIA_TYPE_AUDIO:
                                this.AVStreams.Add(new MediaInfoPropAudioStream(i, pAVStream, pFormatContext));
                                break;
                            case AVMediaType.AVMEDIA_TYPE_SUBTITLE:
                                this.AVStreams.Add(new MediaInfoPropStream(i, pAVStream, pFormatContext));
                                break;
                            case AVMediaType.AVMEDIA_TYPE_ATTACHMENT:
                                this.AVStreams.Add(new MediaInfoPropStream(i, pAVStream, pFormatContext));
                                break;
                            case AVMediaType.AVMEDIA_TYPE_DATA:
                                this.AVStreams.Add(new MediaInfoPropStream(i, pAVStream, pFormatContext));
                                break;
                            case AVMediaType.AVMEDIA_TYPE_NB:
                                this.AVStreams.Add(new MediaInfoPropStream(i, pAVStream, pFormatContext));
                                break;
                            default:
                                this.AVStreams.Add(new MediaInfoPropStream(i, pAVStream, pFormatContext));
                                break;
                        }
                    }
                }
                finally
                {
                    ffmpeg.avformat_close_input(&pFormatContext);
                }
            }
            finally
            {
                ffmpeg.avformat_free_context(pFormatContext);
            }
        }

        public static unsafe string av_strerror(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }

        #region Conversions

        public unsafe static Dictionary<string, string> ToDictionary(AVDictionary* dict)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AVDictionaryEntry* tag = null;
            while ((tag = ffmpeg.av_dict_get(dict, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                result.Add(Marshal.PtrToStringAnsi((IntPtr)tag->key), Marshal.PtrToStringAnsi((IntPtr)tag->value));
            }
            return result;
        }

        public static string ToFourCCString(uint value)
        {
            return String.Join("", (new byte[]
            {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 24) & 0xFF)
            }).Select(b =>
                (Char.IsLetterOrDigit(Convert.ToChar(b)) || Char.IsPunctuation(Convert.ToChar(b)) || Char.IsSeparator(Convert.ToChar(b)) || Char.IsWhiteSpace(Convert.ToChar(b))) ? Convert.ToChar(b).ToString() : String.Format("[{0}]", b.ToString("x2"))
            ).ToArray());
        }

        public static double ToDouble(AVRational value)
        {
            return Convert.ToDouble(value.num) / Convert.ToDouble(value.den);
        }
        public static string ToDescString(AVRational value)
        {
            double dVal = Math.Floor(ToDouble(value) * 10.0) / 10;
            return dVal.ToString();
        }

        public static TimeSpan ToTimeSpan(long value)
        {
            return new TimeSpan(value * 10L);
        }
        public static TimeSpan ToTimeSpan(long value, double timeBase)
        {
            return TimeSpan.FromSeconds(Convert.ToDouble(value) * timeBase);
        }

        public static string ToFormattedString(TimeSpan s)
        {
            return String.Format("{0:00}:{1:00}:{2:00}.{3:000}", s.Hours, s.Minutes, s.Seconds, s.Milliseconds);
        }

        public static string GetStandard(int width, int height)
        {
            // standard
            if ((width > 7680) & (height > 4320) & (width <= 11520) & (height <= 6480))
                return "12K";
            else if ((width > 3840) & (height > 2160))
                return "8K";
            else if ((width > 2560) & (height > 1600))
                return "4K";
            else if ((width > 1920) & (height > 1200))
                return "2K";
            else if ((width > 1920) & (height > 1080))
                return "WUXGA";
            else if ((width > 1280) & (height > 720))
                return "HD";
            else if ((width > 720) & (height > 576))
                return "HD";
            else if ((width >= 720) & (height >= 480))
                return "SD";
            else
                return "Unknown";
        }


        /**
            Width	Height	Standard
            256	144	
            426	240	
            640	360	nHD
            768	432	
            800	450	
            848	480	
            854	480	FWVGA
            960	540	qHD
            1024	576	
            1280	720	HD
            1366	768	WXGA
            1600	900	HD+
            1920	1080	Full HD
            2048	1152	
            2560	1440	QHD
            2880	1620	
            3200	1800	QHD+
            3840	2160	4K UHD
            4096	2304	
            5120	2880	5K
            7680	4320	8K UHD
            15360	8640	16K UHD
        **/

        /**
            bmdModeNTSC 			720 486 30/1.001 2 30000 1001
            bmdModeNTSC2398 		720 486 30/1.001* 2 24000* 1001
            bmdModeNTSCp 			720 486 60/1.001 1 60000 1001
            bmdModePAL 				720 576 25 2 25000 1000
            bmdModePALp 			720 576 50 1 50000 1000
            bmdModeHD720p50 		1280 720 50 1 50000 1000
            bmdModeHD720p5994 		1280 720 60/1.001 1 60000 1001
            bmdModeHD720p60 		1280 720 60 1 60000 1000
            bmdModeHD1080p2398 		1920 1080 24/1.001 1 24000 1001
            bmdModeHD1080p24 		1920 1080 24 1 24000 1000
            bmdModeHD1080p25 		1920 1080 25 1 25000 1000
            bmdModeHD1080p2997 		1920 1080 30/1.001 1 30000 1001
            bmdModeHD1080p30 		1920 1080 30 1 30000 1000
            bmdModeHD1080i50 		1920 1080 25 2 25000 1000
            bmdModeHD1080i5994 		1920 1080 30/1.001 2 30000 1001
            bmdModeHD1080i6000 		1920 1080 30 2 30000 1000
            bmdModeHD1080p50 		1920 1080 50 1 50000 1000
            bmdModeHD1080p5994 		1920 1080 60/1.001 1 60000 1001
            bmdModeHD1080p6000 		1920 1080 60 1 60000 1000
            bmdMode2k2398 			2048 1556 24/1.001 1 24000 1001
            bmdMode2k24 			2048 1556 24 1 24000 1000
            bmdMode2k25 			2048 1556 25 1 25000 1000
            bmdMode2kDCI2398 		2048 1080 24/1.001 1 24000 1001
            bmdMode2kDCI24 			2048 1080 24 1 24000 1000
            bmdMode2kDCI25 			2048 1080 25 1 25000 1000
            bmdMode4K2160p2398 		3840 2160 24/1.001 1 24000 1001
            bmdMode4K2160p24 		3840 2160 24 1 24000 1000
            bmdMode4K2160p25 		3840 2160 25 1 25000 1000
            bmdMode4K2160p2997 		3840 2160 30/1.001 1 30000 1001
            bmdMode4K2160p30 		3840 2160 30 1 30000 1000
            bmdMode4K2160p50 		3840 2160 50 1 50000 1000
            bmdMode4K2160p5994	 	3840 2160 60/1.001 1 60000 1001
            bmdMode4K2160p60	 	3840 2160 60 1 60000 1000
            bmdMode4kDCI2398	 	4096 2160 24/1.001 1 24000 1001
            bmdMode4kDCI24	 		4096 2160 24 1 24000 1000
            bmdMode4kDCI25 			4096 2160 25 1 25000 1000
        **/

        public enum VideoModes
        {
            ModeUnknown,
            ModeNTSC,
            ModeNTSC2398,
            ModeNTSCp,
            ModePAL,
            ModePALp,
            ModeHD720p50,
            ModeHD720p5994,
            ModeHD720p60,
            ModeHD1080p2398,
            ModeHD1080p24,
            ModeHD1080p25,
            ModeHD1080p2997,
            ModeHD1080p30,
            ModeHD1080i50,
            ModeHD1080i5994,
            ModeHD1080i6000,
            ModeHD1080p50,
            ModeHD1080p5994,
            ModeHD1080p6000,
            Mode2k2398,
            Mode2k24,
            Mode2k25,
            Mode2kDCI2398,
            Mode2kDCI24,
            Mode2kDCI25,
            Mode4K2160p2398,
            Mode4K2160p24,
            Mode4K2160p25,
            Mode4K2160p2997,
            Mode4K2160p30,
            Mode4K2160p50,
            Mode4K2160p5994,
            Mode4K2160p60,
            Mode4kDCI2398,
            Mode4kDCI24,
            Mode4kDCI25
        };


        public unsafe static VideoModes GetVideoModes(AVFormatContext* pAVFormatContext, AVStream* pAVStream)
        {
            // display_aspect_ratio, sample_aspect_ratio
            AVRational _sar = ffmpeg.av_guess_sample_aspect_ratio(pAVFormatContext, pAVStream, null);
            if (_sar.num > 0)
            {
                AVRational _dar = new AVRational();
                ffmpeg.av_reduce(&_dar.num, &_dar.den, pAVStream->codecpar->width * _sar.num, pAVStream->codecpar->height * _sar.den, 1024 * 1024);
                string friendlyName = MediaInfo.ToAspectRatioString(_dar.num, _dar.den);
            }

            int Width = pAVStream->codecpar->width;
            int Height = pAVStream->codecpar->height;
            double FramesPerSecond = Math.Round(ffmpeg.av_q2d(pAVStream->avg_frame_rate), 2);
            int FieldsPerFrame = pAVStream->codecpar->field_order == AVFieldOrder.AV_FIELD_PROGRESSIVE ? 2 : 1;

            /**
                AVCHD_1920x1080i_2997
                width=1920
                height=1080
                sample_aspect_ratio=1:1
                display_aspect_ratio=16:9
                r_frame_rate=60000/1001
                avg_frame_rate=30000/1001
                time_base=1001/60000

                NTSC_SD_DV25_colorbar
                width=720
                height=480
                coded_width=720
                coded_height=480
                has_b_frames=0
                sample_aspect_ratio=10:11
                display_aspect_ratio=15:11
                r_frame_rate=2997/100
                avg_frame_rate=2997/100
                time_base=1/2997
            **/


            return VideoModes.ModeUnknown;


        }

        public static string GetStandard1(int width, int height, AVRational sar)
        {

            int width_ = (width * sar.num / sar.den);


            // standard
            if ((width > 7680) & (height > 4320) & (width <= 11520) & (height <= 6480))
                return "12K";
            else if ((width > 3840) & (height > 2160))
                return "8K";
            else if ((width > 2560) & (height > 1600))
                return "4K";
            else if ((width > 1920) & (height > 1200))
                return "2K";
            else if ((width > 1920) & (height > 1080))
                return "WUXGA";
            else if ((width > 1280) & (height > 720))
                return "HD";
            else if ((width > 720) & (height > 576))
                return "HD";
            else if ((width >= 720) & (height >= 480))
                return "SD";
            else
                return "Unknown";
        }


        public static string ToAspectRatioString(int dar_num, int dar_den)
        {
            switch (Math.Round((Double)dar_num / (Double)dar_den, 2))
            {
                case 1.25:
                    return "5:4";
                case 1.33:
                    return "4:3";
                case 1.37:
                    return "4:3, (Academy Format)";
                case 1.4:
                    return "14:10, (IMAX Film)";
                case 1.6:
                    return "16:10";
                case 1.77:
                case 1.78:
                    return "16:9";
                case 1.82:
                    return "16:9, (4:3 anamorphic)";
                case 1.85:
                    return "16:9, (Academy Format)";
                case 1.9:
                    return "19:10, (IMAX Digital)";
                case 2.35:
                case 2.36:
                    return "21:9, (CINEMASCOPE)";
                default:
                    return String.Format("{0}:{1}", dar_num, dar_den);
            }
        }
        #endregion
    }
}



