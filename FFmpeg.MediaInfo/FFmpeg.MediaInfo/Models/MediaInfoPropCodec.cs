using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.Models
{
    public struct MediaInfoPropCodec
    {
        public string? String { get; set; }

        public string? Name { get; set; }

        public string? Profile { get; set; }

        public MediaInfoPropPair<long, string>? Tag { get; set; }
    }
}
