using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpeg.MediaInfo.Models
{
    public struct MediaInfoPropAudioChannels
    {
        public int? Value { get; set; }
        public ulong? Layout { get; set; }
        public string? LayoutName { get; set; }
    }
}
