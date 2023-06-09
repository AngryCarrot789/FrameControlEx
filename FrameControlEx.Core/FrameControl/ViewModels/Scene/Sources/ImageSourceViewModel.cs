using System;
using System.IO;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene.Sources {
    public class ImageSourceViewModel : AVSourceViewModel {
        public new ImageSourceModel Model => (ImageSourceModel) ((BaseIOViewModel) this).Model;

        public bool RequireImageReload {
            get => this.Model.IsFilePathDirty;
            set {
                this.Model.IsFilePathDirty = value;
                this.RaisePropertyChanged();
            }
        }

        public string FilePath {
            get => this.Model.FilePath;
            set {
                this.Model.FilePath = value;
                this.RaisePropertyChanged();
                this.RequireImageReload = true; // just in case FilePath is bound to a text box or something
            }
        }

        public AsyncRelayCommand SelectFileCommand { get; }

        public AsyncRelayCommand RefreshCommand { get; }

        public ImageSourceViewModel(ImageSourceModel model) : base(model) {
            this.SelectFileCommand = new AsyncRelayCommand(this.SelectFileActionAsync);
            this.RefreshCommand = new AsyncRelayCommand(this.RefreshActionAsync);
        }

        public async Task RefreshActionAsync() {
            if (string.IsNullOrEmpty(this.FilePath)) {
                await IoC.MessageDialogs.ShowMessageAsync("Empty file path", "The image path input is empty");
                return;
            }

            if (!File.Exists(this.FilePath)) {
                await IoC.MessageDialogs.ShowMessageAsync("No such file", $"Image file does not exist: {this.FilePath}");
                return;
            }

            try {
                await this.Model.LoadImageAsync(this.FilePath);
                this.RequireImageReload = false;
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error opening image", $"Error opening '{this.FilePath}'", e.GetToString());
            }

            this.Model.InvalidateVisual();
        }

        public async Task SelectFileActionAsync() {
            DialogResult<string[]> result = IoC.FilePicker.ShowFilePickerDialog(Filters.ImageTypesAndAll, this.FilePath, "Select an image to open", false);
            if (result.IsSuccess && result.Value.Length == 1) {
                string path = result.Value[0];
                if (string.IsNullOrEmpty(path)) {
                    return;
                }

                this.Model.FilePath = path;
                this.RaisePropertyChanged(nameof(this.FilePath));
                this.RequireImageReload = false;

                try {
                    await this.Model.LoadImageAsync(path);
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Error opening image", $"Exception occurred while opening {path}", e.GetToString());
                }

                this.Model.InvalidateVisual();
            }
        }
    }
}