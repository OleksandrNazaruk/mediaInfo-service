using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.DTOs
{
    public class MediaInfoPropFormatDTO
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("long_name")]
        public string? LongName { get; set; }

        public MediaInfoPropFormatDTO()
        {

        }

        public static MediaInfoPropFormatDTO Parse(string name, string longName)
        {
            return new MediaInfoPropFormatDTO() 
            {
                Name = name,
                LongName = longName,
            };
        }
    }
}
