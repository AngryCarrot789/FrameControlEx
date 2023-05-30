using SkiaSharp;

namespace FrameControlEx.Core.Utils {
    public abstract class BitmapFactory {
        public static BitmapFactory Instance { get; set; }

        public abstract MutableBitmap NewBitmap(SKImageInfo info);
    }
}