using FFmpeg.AutoGen;
using System.Runtime.InteropServices;

namespace _MediaInfoService
{
    public unsafe class FFmpegLogger
    {
        private readonly av_log_set_callback_callback? _av_log_callback;
        private readonly ILogger<FFmpegLogger> _logger;

        public FFmpegLogger(ILogger<FFmpegLogger> logger, int? level = null, int? flags = null)
        {
            this._logger = logger;

            if (level.HasValue)
            {
                ffmpeg.av_log_set_level(level.Value);
            }

            if (flags.HasValue)
            {
                ffmpeg.av_log_set_flags(flags.Value);
            }

            this._av_log_callback = new av_log_set_callback_callback(av_log_handler);
            ffmpeg.av_log_set_callback(this._av_log_callback);
        }

        private unsafe void av_log_handler(void* p0, int level, [MarshalAs(UnmanagedType.LPUTF8Str)] string format, byte* vl)
        {

            if (level > ffmpeg.av_log_get_level()) return;

            var lineSize = 1024;
            var lineBuffer = stackalloc byte[lineSize];
            var printPrefix = 1;
            ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
            var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);

            switch (ffmpeg.av_log_get_level())
            {
                case ffmpeg.AV_LOG_QUIET:
                    break;
                case ffmpeg.AV_LOG_VERBOSE:
                case ffmpeg.AV_LOG_DEBUG:
                    this._logger?.LogDebug(line);
                    break;
                case ffmpeg.AV_LOG_ERROR:
                    this._logger?.LogError(line);
                    break;
                case ffmpeg.AV_LOG_PANIC:
                case ffmpeg.AV_LOG_FATAL:
                    this._logger?.LogCritical(line);
                    break;
                case ffmpeg.AV_LOG_INFO:
                    this._logger?.LogInformation(line);
                    break;
                case ffmpeg.AV_LOG_TRACE:
                    this._logger?.LogTrace(line);
                    break;
                case ffmpeg.AV_LOG_WARNING:
                    this._logger?.LogWarning(line);
                    break;
            }
        }

    }
}
