using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.Models
{
    public class MediaInfoPropFormat
    {
        public string? Name { get; set; }

        public string? LongName { get; set; }

        public static MediaInfoPropFormat Parse(string name, string longName)
        {
            return new MediaInfoPropFormat() 
            {
                Name = name,
                LongName = longName,
            };
        }
    }
}
