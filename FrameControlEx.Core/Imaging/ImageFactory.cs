using System;
using System.IO;
using System.Threading.Tasks;
using FrameControlEx.Core.Utils;
using SkiaSharp;

namespace FrameControlEx.Core.Imaging {
    /// <summary>
    /// A class for creating and managing images in a cross-platform manner
    /// </summary>
    public class ImageFactory {
        public static async Task<SkiaImage> CreateImageAsync(string path) {
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