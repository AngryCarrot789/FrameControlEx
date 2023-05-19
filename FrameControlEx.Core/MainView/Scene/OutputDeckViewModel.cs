using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene.Outputs;
using FrameControlEx.Core.MainView.Scene.Sources;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.MainView.Scene {
    /// <summary>
    /// A view model for storing all outputs for a specific scene
    /// A view model for storing all outputs for a specific scene
    /// </summary>
    public class OutputDeckViewModel : BaseListDeckViewModel<OutputViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;

        public SceneViewModel Scene { get; }

        // not sure how else to bypass dialogs :/
        public bool InternalBypassDialog { get; set; }

        public AsyncRelayCommand AddSameInstanceOutputCommand { get; }

        public OutputDeckViewModel(SceneViewModel scene) {
            this.Scene = scene;
            this.AddSameInstanceOutputCommand = new AsyncRelayCommand(this.AddSameInstanceOutputAction);
        }

        static OutputDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        private async Task AddSameInstanceOutputAction() {
            BasicBufferOutputViewModel source = new BasicBufferOutputViewModel(this) {
                ReadableName = $"SIOutput {this.items.Count + 1}"
            };

            this.items.Add(source);
        }

        public override async Task AddActionAsync() {
            await IoC.MessageDialogs.ShowMessageAsync("Coming soon", "This feature is coming soon!");
        }

        public override async Task RemoveItemAction(OutputViewModel item) {
            if (!this.items.Contains(item)) {
                return;
            }

            if (!this.InternalBypassDialog) {
                string result = await ConfirmRemoveDialog.ShowAsync("Remove output?", $"Are you sure you want to remove {(string.IsNullOrEmpty(item.ReadableName) ? "this output" : item.ReadableName)}?");
                if (result != "yes") {
                    return;
                }
            }

            await base.RemoveItemAction(item);
        }
    }
}