using System;
using System.Numerics;
using System.Threading.Tasks;
using FrameControlEx.Core.Imaging;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene.Sources {
    public class ImageSourceViewModel : AVSourceViewModel {
        private bool requireImageReload = true;
        public bool RequireImageReload {
            get => this.requireImageReload;
            set => this.RaisePropertyChanged(ref this.requireImageReload, value);
        }

        private string filePath;
        public string FilePath {
            get => this.filePath;
            set {
                this.RaisePropertyChanged(ref this.filePath, value);
                this.RequireImageReload = true; // just in case FilePath is bound to a text box or something
            }
        }

        /// <summary>
        /// The image loaded from a file, stored in memory. May be null (duh)
        /// </summary>
        public IImage Image { get; set; }

        public AsyncRelayCommand SelectFileCommand { get; }

        public AsyncRelayCommand RefreshCommand { get; }

        public ImageSourceViewModel() {
            this.SelectFileCommand = new AsyncRelayCommand(this.SelectFileActionAsync);
            this.RefreshCommand = new AsyncRelayCommand(this.RefreshActionAsync);
        }

        public async Task RefreshActionAsync() {
            try {
                await this.OpenImage(this.FilePath);
                this.RequireImageReload = false;
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error opening image", $"Error opening '{this.filePath}'", e.GetToString());
            }

            this.InvalidateVisual();
        }

        public async Task SelectFileActionAsync() {
            DialogResult<string[]> result = IoC.FilePicker.ShowFilePickerDialog(Filters.ImageTypesAndAll, this.FilePath, "Select an image to open", false);
            if (result.IsSuccess && result.Value.Length == 1) {
                string path = result.Value[0];
                if (string.IsNullOrEmpty(path)) {
                    return;
                }

                this.filePath = path;
                this.RaisePropertyChanged(nameof(this.FilePath));
                this.RequireImageReload = false;

                try {
                    await this.OpenImage(path);
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Error opening image", $"Exception occurred while opening {path}", e.GetToString());
                }

                this.InvalidateVisual();
            }
        }

        public override Vector2 GetRawSize() {
            if (this.Image is ImageFactory.SkiaImage img) {
                return new Vector2(img.image.Width, img.image.Height);
            }
            else {
                return Vector2.Zero;
            }
        }

        public override void OnRender(RenderContext context) {
            base.OnRender(context);
            if (this.Image is ImageFactory.SkiaImage img) {
                Vector2 scale = this.Scale, pos = this.Pos, origin = this.ScaleOrigin;
                SKMatrix matrix = context.Canvas.TotalMatrix;
                context.Canvas.Translate(pos.X, pos.Y);
                context.Canvas.Scale(scale.X, scale.Y, img.image.Width * origin.X, img.image.Height * origin.Y);
                context.Canvas.DrawImage(img.image, 0, 0);
                context.Canvas.SetMatrix(matrix);
            }
        }

        private async Task OpenImage(string file) {
            IImage image = await ImageFactory.CreateImageAsync(file);
            if (this.Image != null) {
                #if DEBUG
                this.Image.Dispose();
                #else // lazy
                try {
                    this.Image.Dispose();
                }
                catch { /* ignored */ }
                #endif
            }

            this.Image = image;
        }

        protected override void DisposeCore(ExceptionStack e) {
            base.DisposeCore(e);
            if (this.Image != null) {
                try {
                    this.Image.Dispose();
                }
                catch (Exception ex) {
                    e.Push(ex);
                }
            }
        }

        protected override BaseIOViewModel CreateInstanceCore() {
            return new ImageSourceViewModel();
        }

        protected override void LoadThisIntoUserCopy(BaseIOViewModel vm) {
            base.LoadThisIntoUserCopy(vm);
            if (vm is ImageSourceViewModel src) {
                src.filePath = this.filePath;
            }
        }
    }
}