using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using FrameControlEx.Core.Imaging;
using FrameControlEx.Core.Utils;
using SkiaSharp;

namespace FrameControlEx.Imaging {
    public class SkiaImageFactory : ImageFactory {
        public override async Task<IImage> CreateImageAsync(string path) {
            using (FileStream stream = File.OpenRead(path)) {
                using (MemoryStream memory = new MemoryStream()) {
                    await stream.CopyToAsync(memory);
                    memory.Seek(0, SeekOrigin.Begin);
                    SKBitmap bitmap = await Task.Run(() => SKBitmap.Decode(memory));
                    return new SkiaImage(bitmap);
                }
            }
        }

        public class SkiaImage : IImage, IDisposable {
            public readonly SKBitmap bitmap;
            public readonly SKImage image;

            public SkiaImage(SKBitmap bitmap) {
                this.bitmap = bitmap;
                this.image = SKImage.FromBitmap(bitmap);
            }

            public void Dispose() {
                using (ExceptionStack stack = ExceptionStack.Push("Exception while disposing IImage implementation (SkiaImage) bitmap and image")) {
                    try {
                        this.bitmap.Dispose();
                    }
                    catch (Exception e) {
                        stack.Push(e);
                    }

                    try {
                        this.image.Dispose();
                    }
                    catch (Exception e) {
                        stack.Push(e);
                    }
                }
            }
        }
    }
}