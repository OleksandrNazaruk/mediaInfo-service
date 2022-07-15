using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.DTOs
{
    public class MediaInfoPropPairDTO<TKey, TValue>
    {
        [JsonPropertyName("value")]
        public TKey? Value { get; set; }

        [JsonPropertyName("string")]
        public TValue? String { get; set; }

        public MediaInfoPropPairDTO()
        {

        }

        public static MediaInfoPropPairDTO<TKey, TValue> Parse(TKey Value, TValue String)
        {
            return new MediaInfoPropPairDTO<TKey, TValue>()
            {
                Value = Value,
                String = String,
            };
        }

        public static MediaInfoPropPairDTO<TKey, TValue> Parse((TKey Value, TValue String) pair)
        {
            return new MediaInfoPropPairDTO<TKey, TValue>()
            {
                Value = pair.Value,
                String = pair.String,
            };
        }
    }
}
