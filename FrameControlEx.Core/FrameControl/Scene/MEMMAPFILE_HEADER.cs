using System.Runtime.InteropServices;

namespace FrameControlEx.Core.FrameControl.Scene.Outputs {
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMMAPFILE_HEADER {
        public int width;
        public int height;
        public short bpp;
        private short unused0;
        private int unused1;
        private long unused2;
        private long unused3;
        private long unused4;
        private long unused5;
        private long unused6;
        private long unused7;
    }
}