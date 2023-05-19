using System.Threading.Tasks;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.MainView.Scene.InOuts {
    /// <summary>
    /// A view model for storing all outputs for a specific scene
    /// A view model for storing all outputs for a specific scene
    /// </summary>
    public class OutputDeckViewModel : BaseListDeckViewModel<OutputViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;

        public SceneViewModel Scene { get; }

        public OutputDeckViewModel(SceneViewModel scene) {
            this.Scene = scene;
        }

        static OutputDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        public override async Task AddAction() {
            await IoC.MessageDialogs.ShowMessageAsync("Coming soon", "This feature is coming soon!");
        }

        public override async Task RemoveItemAction(OutputViewModel item) {
            if (!this.items.Contains(item)) {
                return;
            }

            string result = await ConfirmRemoveDialog.ShowAsync("Remove output?", $"Are you sure you want to remove {(string.IsNullOrEmpty(item.ReadableName) ? "this output" : item.ReadableName)}?");
            if (result == "yes") {
                await base.RemoveItemAction(item);
            }
        }
    }
}