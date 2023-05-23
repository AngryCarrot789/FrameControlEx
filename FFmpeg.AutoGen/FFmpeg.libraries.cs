

// ReSharper disable once CheckNamespace
namespace FFmpeg.AutoGen
{
    using System.Collections.Generic;
    
    public unsafe static partial class ffmpeg
    {
        public static Dictionary<string, int> LibraryVersionMap =  new Dictionary<string, int>
        {
            {"avcodec", 60},
            {"avdevice", 60},
            {"avfilter", 9},
            {"avformat", 60},
            {"avutil", 58},
            {"postproc", 57},
            {"swresample", 4},
            {"swscale", 7},
        };
    }
}
