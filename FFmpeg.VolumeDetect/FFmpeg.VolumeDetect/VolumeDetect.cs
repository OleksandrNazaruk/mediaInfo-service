using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using FFmpeg.AudioStreamDecoder;
using FFmpeg.AutoGen;
using FFmpeg.VolumeDetect.Models;

namespace FFmpeg.VolumeDetect
{
    public class VolumeDetect
    {
        public const int MAX_DB = 91;

        public unsafe static List<AudioMeter> Process(string url)
        {
            List<AudioMeter> audioMeters = new List<AudioMeter>();
            Dictionary<int, UInt64[]> histograms = new Dictionary<int, UInt64[]>();

            //
            using AudioDecoder audioDecoder = new AudioDecoder(url, AVSampleFormat.AV_SAMPLE_FMT_S16);
            {
                audioDecoder.Decode((AVFrame* samples, int streamIndex) =>
                {
                    UInt64[] histogram;
                    if (!histograms.TryGetValue(streamIndex, out histogram))
                    {
                        histogram = new UInt64[0x10001];
                        histograms.TryAdd(streamIndex, histogram);
                    }

                    int nb_samples = samples->nb_samples;
                    int nb_channels = samples->channels;
                    int nb_planes = nb_channels;
                    Int16* pcm;

                    if (ffmpeg.av_sample_fmt_is_planar((AVSampleFormat)samples->format) == 0)
                    {
                        nb_samples *= nb_channels;
                        nb_planes = 1;
                    }

                    for (int plane = 0; plane < nb_planes; plane++)
                    {
                        pcm = (Int16*)samples->extended_data[plane];
                        for (int i = 0; i < nb_samples; i++)
                            histogram[pcm[i] + 0x8000]++;
                    }
                });
            }


            // GetAudioMeterResult
            foreach (KeyValuePair<int, UInt64[]> histogram in histograms)
            {
                AudioMeter audioMeter = new AudioMeter(histogram.Key);

                int i, max_volume, shift;
                UInt64 nb_samples = 0, power = 0, nb_samples_shift = 0, sum = 0;
                UInt64[] histdb = new ulong[MAX_DB + 1];

                for (i = 0; i < 0x10000; i++)
                    nb_samples += histogram.Value[i];
                audioMeter.Samples = nb_samples;
                Console.WriteLine(String.Format("nb_samples: {0:d}", nb_samples));

                if (nb_samples == 0)
                {
                    audioMeters.Add(audioMeter);
                    continue;
                }

                /* If nb_samples > 1<<34, there is a risk of overflow in the
                   multiplication or the sum: shift all histogram values to avoid that.
                   The total number of samples must be recomputed to avoid rounding
                   errors. */
                shift = ffmpeg.av_log2((uint)(nb_samples >> 33));
                for (i = 0; i < 0x10000; i++)
                {
                    nb_samples_shift += histogram.Value[i] >> shift;
                    power += (ulong)(i - 0x8000) * (ulong)(i - 0x8000) * (histogram.Value[i] >> shift);
                }
                if (nb_samples_shift == 0)
                {
                    audioMeters.Add(audioMeter);
                    continue;
                }
                power = (power + nb_samples_shift / 2) / nb_samples_shift;
                // av_assert0(power <= 0x8000 * 0x8000);
                audioMeter.MeanVolume = -logdb(power);
                Console.WriteLine(String.Format("mean_volume: {0:.0} dB", -logdb(power)));


                max_volume = 0x8000;
                while (max_volume > 0 && histogram.Value[0x8000 + max_volume] == 0 && histogram.Value[0x8000 - max_volume] == 0)
                    max_volume--;

                audioMeter.MaxVolume = -logdb((ulong)(max_volume * max_volume));
                Console.WriteLine(String.Format("max_volume: {0:.0} dB", -logdb((ulong)(max_volume * max_volume))));

                for (i = 0; i < 0x10000; i++)
                    histdb[(int)logdb((ulong)((i - 0x8000) * (i - 0x8000)))] += histogram.Value[i];
                for (i = 0; i <= MAX_DB && histdb[(ulong)i] == 0; i++) ;
                for (; i <= MAX_DB && sum < nb_samples / 1000; i++)
                {
                    audioMeter.Histogram.TryAdd(String.Format("{0:d}db", i), histdb[i]);
                    Console.WriteLine(String.Format("histogram_{0:d}db: {1:d}", i, histdb[i]));
                    sum += histdb[i];
                }

                audioMeters.Add(audioMeter);
            }

            return audioMeters;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double logdb(UInt64 v)
        {
            double d = v / (double)(0x8000 * 0x8000);
            return v == 0 ? MAX_DB : -Math.Log10(d) * 10;
        }
    }
}
