using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.FrameControl.Scene.Sources {
    /// <summary>
    /// A memory-mapped frame source view model. This reads the current frame from a memory-mapped file. Memory mapped files are
    /// typically accessible across processes, and this class does not care as long as you provide a name
    /// </summary>
    public class MMFAVSourceViewModel : AVSourceViewModel {
        public new MMFSourceModel Model => (MMFSourceModel) ((BaseIOViewModel) this).Model;

        public string MapName {
            get => this.Model.MapName;
            private set {
                this.Model.MapName = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateFile();
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

        public MMFAVSourceViewModel(MMFSourceModel model) : base(model) {
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
        }
    }
}