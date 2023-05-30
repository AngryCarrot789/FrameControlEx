using System.Runtime.InteropServices;

namespace FrameControlEx.Core.FrameControl.Models {
    [StructLayout(LayoutKind.Sequential, Size = 128, Pack = 0)]
    public struct MemMapFileHeader {
        public bool isValid;
        public int width;
        public int height;
        public short bpp;
        public long time;
    }
}