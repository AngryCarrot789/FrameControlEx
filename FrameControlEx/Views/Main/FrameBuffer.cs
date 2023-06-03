using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FrameControlEx.Core.Utils;
using SkiaSharp;

namespace FrameControlEx.Views.Main {
    public class FrameBuffer {
        private volatile WriteableBitmap bitmap;
        private volatile IntPtr backBuffer;
        private readonly CASLock drawLock;
        private SKImageInfo frameInfo;

        public int Width => this.frameInfo.Width;
        
        public int Height => this.frameInfo.Height;

        public WriteableBitmap Bitmap => this.bitmap;

        public FrameBuffer(string debugName = null) {
            this.drawLock = new CASLock(debugName);
        }

        public bool Lock(bool force) {
            return this.drawLock.Lock(force);
        }

        public void Unlock() {
            this.drawLock.Unlock();
        }

        public bool UpdateBitmap(SKImageInfo info) {
            if (!this.drawLock.Lock(true)) {
                return false;
            }

            try {
                this.frameInfo = info;
                this.bitmap = new WriteableBitmap(info.Width, info.Height, 96d, 96d, PixelFormats.Pbgra32, null);
                this.bitmap.Lock();
                this.backBuffer = this.bitmap.BackBuffer;
                this.bitmap.Unlock();
            }
            finally {
                this.drawLock.Unlock();
            }

            return true;
        }

        public bool GetBackBuffer(out IntPtr buffer) {
            if (this.bitmap == null || this.backBuffer == IntPtr.Zero) {
                buffer = IntPtr.Zero;
                return false;
            }

            buffer = this.backBuffer;
            return true;
        }
    }
}