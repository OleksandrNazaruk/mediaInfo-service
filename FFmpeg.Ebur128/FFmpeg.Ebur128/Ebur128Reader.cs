using FFmpeg.AudioStreamDecoder;
using FFmpeg.AutoGen;
using libebur128_net;
using libebur128_net.libebur128;
using System.Runtime.InteropServices;

namespace FFmpeg.Ebur128
{
    public class Ebur128Reader
    {
        public unsafe static void ProcessS16(string url)
        {
            using AudioDecoder audioDecoder = new AudioDecoder(url, AVSampleFormat.AV_SAMPLE_FMT_S16);
            {
                var col = new Dictionary<int, libebur128_net.Ebur128>();

                foreach (var stream in audioDecoder.Streams.Values)
                {

                    libebur128_net.Ebur128 ebur128 = libebur128_net.Ebur128.Init((uint)stream.Channels, (uint)stream.SampleRate,
                        libebur128Native.mode.EBUR128_MODE_I |
                        libebur128Native.mode.EBUR128_MODE_TRUE_PEAK |
                        libebur128Native.mode.EBUR128_MODE_HISTOGRAM |
                        libebur128Native.mode.EBUR128_MODE_LRA);

                        //
                        ebur128.SetChannelType(0, libebur128Native.channel.EBUR128_DUAL_MONO);


                    /* example: set channel map (note: see ebur128.h for the default map) */
                    if (stream.Channels == 5)
                    {
                        ebur128.SetChannelType(0, libebur128Native.channel.EBUR128_LEFT);
                        ebur128.SetChannelType(1, libebur128Native.channel.EBUR128_RIGHT);
                        ebur128.SetChannelType(2, libebur128Native.channel.EBUR128_CENTER);
                        ebur128.SetChannelType(3, libebur128Native.channel.EBUR128_LEFT_SURROUND);
                        ebur128.SetChannelType(4, libebur128Native.channel.EBUR128_RIGHT_SURROUND);
                    }
                    col.Add(stream.Index, ebur128);
                }


                audioDecoder.Decode((AVFrame* samples, int streamIndex) =>
                {
                    int samplesCount = samples->nb_samples;
                    int channelCount = samples->channels;
                    int planesCount = channelCount;

                    if (ffmpeg.av_sample_fmt_is_planar((AVSampleFormat)samples->format) <= 0)
                    {
                        samplesCount *= channelCount;
                        planesCount = 1;
                    }

                    Int16* pcm;
                    Span<float> buffer = new float[samplesCount];
                    for (int plane = 0; plane < planesCount; plane++)
                    {
                        pcm = (Int16*)samples->extended_data[plane];
                        for (int i = 0; i < samplesCount; i++)
                        {
                            buffer[i] = pcm[i] / 32768f;
                        }
                    }
                    col[streamIndex].AddFramesFloat(buffer, (uint)(samplesCount / channelCount));
                });



                double result = default;

                result = libebur128_net.Ebur128.LoudnessGlobalMultiple(col.Values.ToArray());
                Console.WriteLine(String.Format("Integrated: {0,10:0.##} LUFS", result));

                result = libebur128_net.Ebur128.LoudnessRangeMultiple(col.Values.ToArray());
                Console.WriteLine(String.Format("LRA: {0,10:0.##} LU", result));


                foreach (var ebur128 in col.Values)
                {
                    result = ebur128.LoudnessGlobal();
                    Console.WriteLine(String.Format("Integrated: {0,10:0.##} LUFS", result));


                    result = ebur128.RelativeThreshold();
                    Console.WriteLine(String.Format("Threshold: {0,10:0.##} LUFS", result));

                    result = ebur128.LoudnessRange();
                    Console.WriteLine(String.Format("LRA: {0,10:0.##} LU", result));


                    result = ebur128.LoudnessMomentary();
                    Console.WriteLine(String.Format("Momentary Max: {0,10:0.##} LU", result));


                    result = ebur128.LoudnessShortterm();
                    Console.WriteLine(String.Format("Short-term Max: {0,10:0.##} LU", result));


                    for (uint channel = 0; channel < ebur128.Channels; channel++ )
                    {
                        result = ebur128.TruePeak(channel);
                        var truePeak = 20 * Math.Log10(result);
                        Console.WriteLine(String.Format("Channel[{0}]True Peak: {1,10:0.##} dBTP", channel, truePeak));

                        result = ebur128.SamplePeak(channel);
                        var samplePeak = 20 * Math.Log10(result);
                        Console.WriteLine(String.Format("Channel[{0}]Sample Peak: {1,10:0.##} dBFS", channel, samplePeak));
                    }

                    result = ebur128.AbsoluteTruePeak();
                    var maxTruePeak = 20 * Math.Log10(result);
                    Console.WriteLine(String.Format("MAX True Peak: {0,10:0.##} dBTP", maxTruePeak));

                    result = ebur128.AbsoluteSamplePeak();
                    var maxSamplePeak = 20 * Math.Log10(result);
                    Console.WriteLine(String.Format("MAX Sample Peak: {0,10:0.##} dBFS", maxSamplePeak));
                }

                // Dispose
                foreach (var ebur128 in col.Values)
                {
                    ebur128.Dispose();
                }
                col.Clear();
            }
        }

        public unsafe static void Process(string url)
        {
            using AudioDecoder audioDecoder = new AudioDecoder(url);
            {
                var col = new Dictionary<int, libebur128_net.Ebur128>();

                foreach (var stream in audioDecoder.Streams.Values)
                {

                    libebur128_net.Ebur128 ebur128 = libebur128_net.Ebur128.Init((uint)stream.Channels, (uint)stream.SampleRate,
                        libebur128Native.mode.EBUR128_MODE_I |
                        libebur128Native.mode.EBUR128_MODE_TRUE_PEAK |
                        libebur128Native.mode.EBUR128_MODE_HISTOGRAM |
                        libebur128Native.mode.EBUR128_MODE_LRA);

                    //
                    ebur128.SetChannelType(0, libebur128Native.channel.EBUR128_DUAL_MONO);


                    /* example: set channel map (note: see ebur128.h for the default map) */
                    if (stream.Channels == 5)
                    {
                        ebur128.SetChannelType(0, libebur128Native.channel.EBUR128_LEFT);
                        ebur128.SetChannelType(1, libebur128Native.channel.EBUR128_RIGHT);
                        ebur128.SetChannelType(2, libebur128Native.channel.EBUR128_CENTER);
                        ebur128.SetChannelType(3, libebur128Native.channel.EBUR128_LEFT_SURROUND);
                        ebur128.SetChannelType(4, libebur128Native.channel.EBUR128_RIGHT_SURROUND);
                    }
                    col.Add(stream.Index, ebur128);
                }


                audioDecoder.Decode((AVFrame* samples, int streamIndex) =>
                {
                    int samplesCount = samples->nb_samples;
                    int channelCount = samples->channels;
                    int planesCount = channelCount;

                    if (ffmpeg.av_sample_fmt_is_planar((AVSampleFormat)samples->format) <= 0)
                    {
                        samplesCount *= channelCount;
                        planesCount = 1;
                    }

                    // size of sample
                    int bytesPerSample = ffmpeg.av_get_bytes_per_sample((AVSampleFormat)samples->format);
                    if (bytesPerSample < 0)
                        throw new Exception("Failed to calculate data size");

                    // sample buffer
                    Span<float> buffer = new float[samplesCount];

                    for (int plane = 0; plane < planesCount; plane++)
                    {

                        switch (bytesPerSample)
                        {
                            // 16 bit
                            case 2:
                                Int16* pcm16 = (Int16*)samples->extended_data[plane];
                                for (int i = 0; i < samplesCount; i++)
                                {
                                    buffer[i] = pcm16[i] / 32768f;
                                }
                                break;
                            // 24 bit
                            case 3:
                                Int32* pcm24 = (Int32*)samples->extended_data[plane];
                                for (int i = 0; i < samplesCount; i++)
                                {
                                    buffer[i] = pcm24[i] / 8388608f;
                                }
                                break;
                            // 32 bit
                            case 4:
                                Int32* pcm32 = (Int32*)samples->extended_data[plane];
                                for (int i = 0; i < samplesCount; i++)
                                {
                                    buffer[i] = pcm32[i] / 2147483648f;
                                }
                                break;
                            // 8 bit
                            default:
                                sbyte* pcm8 = (sbyte*)samples->extended_data[plane];
                                for (int i = 0; i < samplesCount; i++)
                                {
                                    buffer[i] = pcm8[i] / 128;
                                }
                                break;
                        }
                    }
                    col[streamIndex].AddFramesFloat(buffer, (uint)(samplesCount / channelCount));
                });



                double result = default;

                result = libebur128_net.Ebur128.LoudnessGlobalMultiple(col.Values.ToArray());
                Console.WriteLine(String.Format("Multiple Integrated: {0,10:0.##} LUFS", result));

                result = libebur128_net.Ebur128.LoudnessRangeMultiple(col.Values.ToArray());
                Console.WriteLine(String.Format("Multiple LRA: {0,10:0.##} LU", result));


                foreach (var ebur128 in col)
                {
                    result = ebur128.Value.LoudnessGlobal();
                    Console.WriteLine(String.Format("[{0}] Integrated: {1,10:0.##} LUFS", ebur128.Key, result));


                    result = ebur128.Value.RelativeThreshold();
                    Console.WriteLine(String.Format("[{0}] Threshold: {1,10:0.##} LUFS", ebur128.Key, result));

                    result = ebur128.Value.LoudnessRange();
                    Console.WriteLine(String.Format("[{0}] LRA: {1,10:0.##} LU", ebur128.Key, result));


                    result = ebur128.Value.LoudnessMomentary();
                    Console.WriteLine(String.Format("[{0}] Momentary Max: {1,10:0.##} LU", ebur128.Key, result));


                    result = ebur128.Value.LoudnessShortterm();
                    Console.WriteLine(String.Format("[{0}] Short-term Max: {1,10:0.##} LU", ebur128.Key, result));


                    for (uint channel = 0; channel < ebur128.Value.Channels; channel++)
                    {
                        result = ebur128.Value.TruePeak(channel);
                        var truePeak = 20 * Math.Log10(result);
                        Console.WriteLine(String.Format("[{0}][{1}]True Peak: {2,10:0.##} dBTP", ebur128.Key, channel, truePeak));

                        result = ebur128.Value.SamplePeak(channel);
                        var samplePeak = 20 * Math.Log10(result);
                        Console.WriteLine(String.Format("[{0}][{1}]Sample Peak: {2,10:0.##} dBFS", ebur128.Key, channel, samplePeak));
                    }

                    result = ebur128.Value.AbsoluteTruePeak();
                    var maxTruePeak = 20 * Math.Log10(result);
                    Console.WriteLine(String.Format("[{0}] MAX True Peak: {1,10:0.##} dBTP", ebur128.Key, maxTruePeak));

                    result = ebur128.Value.AbsoluteSamplePeak();
                    var maxSamplePeak = 20 * Math.Log10(result);
                    Console.WriteLine(String.Format("[{0}] MAX Sample Peak: {1,10:0.##} dBFS", ebur128.Key, maxSamplePeak));
                }

                // Dispose
                foreach (var ebur128 in col.Values)
                {
                    ebur128.Dispose();
                }
                col.Clear();
            }
        }

    }
}

