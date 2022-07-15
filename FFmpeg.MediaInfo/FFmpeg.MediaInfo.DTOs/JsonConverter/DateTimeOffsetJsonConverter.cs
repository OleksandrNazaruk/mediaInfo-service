using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpeg.MediaInfo.DTOs.JsonConverter
{
    public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                DateTimeOffset.Parse(
                    reader.GetString());

        public override void Write(
            Utf8JsonWriter writer,
            DateTimeOffset dateTimeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(
                    dateTimeValue.ToUniversalTime().ToString("o"));
    }
}
