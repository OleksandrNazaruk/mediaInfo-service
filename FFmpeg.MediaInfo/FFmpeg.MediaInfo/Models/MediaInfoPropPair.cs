using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.Models
{
    public class MediaInfoPropPair<TKey, TValue>
    {
        public TKey? Value { get; set; } = default;
        public TValue? String { get; set; }

        public static MediaInfoPropPair<TKey, TValue> Parse(TKey Value, TValue String)
        {
            return new MediaInfoPropPair<TKey, TValue>()
            {
                Value = Value,
                String = String,
            };
        }

    }
}
