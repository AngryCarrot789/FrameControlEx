using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base;
using FrameControlEx.Core.RBC;
using FrameControlEx.Core.Utils;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Models.Scene.Sources {
    public class ImageSourceModel : AVSourceModel {
        public string FilePath { get; set; }
        public bool IsFilePathDirty { get; set; }

        private SKBitmap bitmap;
        private SKImage image;

        public ImageSourceModel() {
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.FilePath = data.GetString(nameof(this.FilePath), null);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (!string.IsNullOrEmpty(this.FilePath))
                data.SetString(nameof(this.FilePath), this.FilePath);
        }

        public override Vector2 GetSize() {
            if (this.image == null)
                return Vector2.Zero;
            return new Vector2(this.image.Width, this.image.Height);
        }

        public override void OnRender(RenderContext context) {
            base.OnRender(context);
            if (this.image == null) {
                return;
            }

            Vector2 scale = this.Scale, pos = this.Pos, origin = this.ScaleOrigin;
            SKMatrix matrix = context.Canvas.TotalMatrix;
            context.Canvas.Translate(pos.X, pos.Y);
            context.Canvas.Scale(scale.X, scale.Y, this.image.Width * origin.X, this.image.Height * origin.Y);
            context.Canvas.DrawImage(this.image, 0, 0);
            context.Canvas.SetMatrix(matrix);
        }

        protected override void DisposeCore(ExceptionStack stack) {
            base.DisposeCore(stack);
            try {
                this.bitmap?.Dispose();
            }
            catch (Exception e) {
                stack.Push(new Exception("Failed to dispose bitmap", e));
            }

            try {
                this.image?.Dispose();
            }
            catch (Exception e) {
                stack.Push(new Exception("Failed to dispose image", e));
            }
        }

        public async Task LoadImageAsync(string file) {
            SKBitmap loadedBitmap = await LoadBitmapAsync(file);
            if (this.bitmap != null || this.image != null) {
                #if DEBUG
                this.bitmap?.Dispose();
                this.image?.Dispose();
                #else // lazy
                this.DisposeImageCareless();
                #endif
            }

            this.bitmap = loadedBitmap;
            this.image = await Task.Run(() => SKImage.FromBitmap(this.bitmap));
        }

        public void DisposeImageCareless() {
            try {
                this.bitmap?.Dispose();
            }
            catch { /* ignored */ }

            try {
                this.image?.Dispose();
            }
            catch { /* ignored */ }
        }

        public static async Task<SKBitmap> LoadBitmapAsync(string path) {
            using (BufferedStream stream = new BufferedStream(File.OpenRead(path), 4096)) {
                return await Task.Run(() => SKBitmap.Decode(stream));
            }
        }
    }
}