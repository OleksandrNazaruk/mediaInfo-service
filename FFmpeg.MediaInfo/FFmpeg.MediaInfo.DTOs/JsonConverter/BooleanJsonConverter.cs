using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpeg.MediaInfo.DTOs.JsonConverter
{
    public class BooleanJsonConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Convert.ToBoolean(reader.GetInt32());
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value ? "1" : "0");
        }

    }
}
