using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.Models
{
    public struct MediaInfoPropAVRational
    {
        public string String { get; set; }

        public int? num { get; set; }

        public int? den { get; set; }
    }
}
