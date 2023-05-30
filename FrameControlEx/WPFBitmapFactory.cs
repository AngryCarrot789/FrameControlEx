using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FrameControlEx.Core.Utils;
using SkiaSharp;

namespace FrameControlEx {
    public class WPFBitmapFactory : BitmapFactory {
        public override MutableBitmap NewBitmap(SKImageInfo info) {
            return new WPFWritableBitmap(info);
        }
    }

    public class WPFWritableBitmap : MutableBitmap {
        public readonly WriteableBitmap bitmap;
        private IntPtr backBuffer;

        public int Width => this.bitmap.PixelWidth;
        public int Height => this.bitmap.PixelHeight;
        public int Stride => this.bitmap.BackBufferStride;
        public int BitsPerPixel => this.Stride / this.Width;

        public WPFWritableBitmap(SKImageInfo info) {
            PixelFormat? format = null;
            switch (info.ColorType) {
                case SKColorType.Unknown: break;
                case SKColorType.Alpha8: format = PixelFormats.Gray8; break;
                case SKColorType.Rgb565: break;
                case SKColorType.Argb4444:; break;
                case SKColorType.Rgba8888: break;
                case SKColorType.Rgb888x: format = PixelFormats.Rgb24; break;
                case SKColorType.Bgra8888: format = PixelFormats.Bgra32; break;
                case SKColorType.Rgba1010102: break;
                case SKColorType.Rgb101010x: break;
                case SKColorType.Gray8: format = PixelFormats.Gray8; break;
                case SKColorType.RgbaF16: format = PixelFormats.Rgba64; break;
                case SKColorType.RgbaF16Clamped: format = PixelFormats.Rgba64; break;
                case SKColorType.RgbaF32: format = PixelFormats.Rgba128Float; break;
                case SKColorType.Rg88: break;
                case SKColorType.AlphaF16: break;
                case SKColorType.RgF16: break;
                case SKColorType.Alpha16: break;
                case SKColorType.Rg1616: break;
                case SKColorType.Rgba16161616: format = PixelFormats.Rgba64; break;
                case SKColorType.Bgra1010102: format = PixelFormats.Pbgra32; break;
                case SKColorType.Bgr101010x: format = PixelFormats.Bgr101010; break;
                default: throw new ArgumentOutOfRangeException();
            }

            if (!(format is PixelFormat f)) {
                throw new Exception($"Unsupported colour type/pixel format: {info.ColorType}");
            }

            this.bitmap = new WriteableBitmap(info.Width, info.Height, 96, 96, f, null);
            this.bitmap.Lock();
            this.backBuffer = this.bitmap.BackBuffer;
        }

        public WPFWritableBitmap(WriteableBitmap bitmap) {
            this.bitmap = bitmap;
        }

        public IntPtr GetBackBuffer() {
            this.bitmap.Lock();
            return this.bitmap.BackBuffer;
        }

        public void ReleaseBackBuffer() {
            throw new NotImplementedException();
        }

        public void ReleaseBackBuffer(int x, int y, int w, int h) {
            this.bitmap.AddDirtyRect(new Int32Rect(x, y, w, h));
            this.bitmap.Unlock();
        }
    }
}