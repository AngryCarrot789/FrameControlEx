using System.Runtime.InteropServices;

namespace FrameControlEx.Core.FrameControl.Models {
    [StructLayout(LayoutKind.Sequential, Size = 128, Pack = 0)]
    public struct MEMMAPFILE_HEADER {
        public bool is_valid;
        public int width;
        public int height;
        public short bpp;
        public long time_written;
    }
}