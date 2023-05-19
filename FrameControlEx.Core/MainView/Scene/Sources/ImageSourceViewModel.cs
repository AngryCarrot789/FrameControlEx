using System;
using System.Threading.Tasks;
using FrameControlEx.Core.Imaging;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs;

namespace FrameControlEx.Core.MainView.Scene.Sources {
    public class ImageSourceViewModel : VisualSourceViewModel {
        private bool requireImageReload;
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

        public IImage Image { get; private set; }

        public AsyncRelayCommand SelectFileCommand { get; }

        public AsyncRelayCommand RefreshCommand { get; }

        public ImageSourceViewModel(SourceDeckViewModel deck, string filePath = null) : base(deck) {
            this.filePath = filePath;
            this.SelectFileCommand = new AsyncRelayCommand(this.SelectFileActionAsync);
            this.RefreshCommand = new AsyncRelayCommand(this.RefreshActionAsync);
        }

        public async Task RefreshActionAsync() {
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

            try {
                await this.OpenImage(this.FilePath);
                this.RequireImageReload = false;
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error opening image", $"Error opening '{this.filePath}'", e.ToString());
            }

            this.OnVisualInvalidated();
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
                    await IoC.MessageDialogs.ShowMessageExAsync("Error opening image", $"Exception occurred while opening {path}", e.ToString());
                }
            }
        }

        private async Task OpenImage(string file) {
            ImageFactory factory = ImageFactory.Factory;
            if (factory == null) {
                throw new Exception("Image factory unavailable");
            }

            this.Image?.Dispose();
            this.Image = await factory.CreateImageAsync(file);
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
    }
}