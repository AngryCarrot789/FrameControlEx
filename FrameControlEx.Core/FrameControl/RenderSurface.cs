using System;
using System.Runtime.InteropServices;

namespace FrameControlEx.Core.FrameControl {
    public class RenderSurface : IDisposable {
        private readonly int width;
        private readonly int height;
        private readonly int bpp;
        private volatile bool isDisposed;

        public IntPtr BackBuffer { get; }

        private RenderSurface(int width, int height, int bpp, IntPtr bitmap) {
            this.width = width;
            this.height = height;
            this.bpp = bpp;
            this.BackBuffer = bitmap;
        }

        ~RenderSurface() {
            this.Dispose(false);
        }

        public static RenderSurface CreateSurface(int width, int height, int bpp) {
            int bytes = checked(width * height * bpp);
            IntPtr ptr = Marshal.AllocHGlobal(bytes);
            return new RenderSurface(width, height, bpp, ptr);
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        public void Dispose(bool isDisposing) {
            if (this.isDisposed) {
                return;
            }

            Marshal.FreeHGlobal(this.BackBuffer);
        }
    }
}