using System;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AudioStreamDecoder;
using FFmpeg.AutoGen;

namespace FFmpeg.AudioStreamDecoder.Example
{
    public class FFmpegBinariesHelper
    {
        internal static void RegisterFFmpegBinaries()
        {
            var current = Environment.CurrentDirectory;
            var probe = Path.Combine("FFmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86");
            while (current != null)
            {
                var ffmpegBinaryPath = Path.Combine(current, probe);
                if (Directory.Exists(ffmpegBinaryPath))
                {
                    Console.WriteLine($"FFmpeg binaries found in: {ffmpegBinaryPath}");
                    ffmpeg.RootPath = ffmpegBinaryPath;
                    return;
                }

                current = Directory.GetParent(current)?.FullName;
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
            Console.WriteLine("Runnung in {0}-bit mode.", Environment.Is64BitProcess ? "64" : "32");

            FFmpegBinariesHelper.RegisterFFmpegBinaries();

            Console.WriteLine($"FFmpeg version info: {ffmpeg.av_version_info()}");

            SetupLogging();

            string url = @"C:\Temp\ATU0032384-1-1.mxf";

            File.Delete(@"c:\temp\ch1.raw");

            unsafe
            {
                using AudioDecoder audioDecoder = new AudioDecoder(url);
                {
                    audioDecoder.Decode((AVFrame* samples, int streamIndex) =>
                    {
                        if (streamIndex == 1)
                        {
                            AVRational time_base = audioDecoder.Streams[streamIndex].Stream->time_base;
                            int channelCount = samples->channels;

                            Console.WriteLine(String.Format("pts:{0:d} pts_time:{1:f} dts:{2:d} dts_time:{3:f} duration:{4:d} duration_time:{5:f} stream_index:{6:d}",
                                samples->pts, (ffmpeg.av_q2d(time_base) * samples->pts),
                                samples->pkt_dts, ffmpeg.av_q2d(time_base) * samples->pkt_dts,
                                samples->pkt_duration, ffmpeg.av_q2d(time_base) * samples->pkt_duration,
                                streamIndex));

                            // size of sample
                            int dataSize = ffmpeg.av_get_bytes_per_sample((AVSampleFormat)samples->format);
                            if (dataSize < 0)
                                throw new Exception("Failed to calculate data size");

                            // calculate one channel size
                            int bufferSize = dataSize * samples->nb_samples;
                            byte[] buffer = new byte[bufferSize];

                            for (int i = 0; i < samples->nb_samples; i++)
                                for (int ch = 0; ch < channelCount; ch++)
                                    Marshal.Copy((IntPtr)samples->data[(uint)ch] + dataSize * i, buffer, dataSize * i, dataSize);

                            using (var stream = new FileStream(@"c:\temp\ch1.raw", FileMode.Append))
                            {
                                stream.Write(buffer, 0, buffer.Length);
                            }
                        }

                    });
                }
            }

            Console.ReadKey();

        }
        private static unsafe void SetupLogging()
        {
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);

            // do not convert to local function
            av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
            {
                if (level > ffmpeg.av_log_get_level()) return;

                var lineSize = 1024;
                var lineBuffer = stackalloc byte[lineSize];
                var printPrefix = 1;
                ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(line);
                Console.ResetColor();
            };

            ffmpeg.av_log_set_callback(logCallback);
        }
    }
}
