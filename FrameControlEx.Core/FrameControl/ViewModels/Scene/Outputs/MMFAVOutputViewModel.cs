using System;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.FrameControl.Scene.Outputs {
    /// <summary>
    /// A memory-mapped file based output. This writes the current frame to a memory-mapped file for other processes to read from
    /// </summary>
    public class MMFAVOutputViewModel : AVOutputViewModel {
        public new MMFOutputModel Model => (MMFOutputModel) ((BaseIOViewModel) this).Model;

        public string MapName {
            get => this.Model.MapName;
            private set {
                this.Model.MapName = value;
                this.RaisePropertyChanged();
                this.IsAutoConnectionFault = false;
            }
        }

        public bool GenerateMapName {
            get => this.Model.GenerateMapName;
            set {
                this.Model.GenerateMapName = value;
                this.RaisePropertyChanged();
                this.IsAutoConnectionFault = false;
            }
        }

        public bool IsAutoConnectionFault {
            get => this.Model.IsAutoConnectionFault;
            set {
                this.Model.IsAutoConnectionFault = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand EditMappedFileNameCommand { get; }

        public MMFAVOutputViewModel(MMFOutputModel model) : base(model) {
            this.EditMappedFileNameCommand = new AsyncRelayCommand(this.EditMappedFileNameAction);
            model.OnFault += (src, fault) => {
                this.RaisePropertyChanged(nameof(this.IsAutoConnectionFault));
            };
        }

        public async Task EditMappedFileNameAction() {
            string result = await IoC.UserInput.ShowSingleInputDialogAsync("New File Name", "Input a new file name for the memory-mapped file. This will invalidate the old one", this.MapName ?? "my_mapped_file", Validators.ForNonEmptyString("File name cannot be an empty string"));
            if (result == null) {
                return;
            }

            this.MapName = result;

            try {
                this.Model.InvalidateFile();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("File disposal error", "Failed to release the current file's resources", e.GetToString());
            }
        }
    }
}