using System.Threading.Tasks;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.MainView.Scene.InOuts {
    /// <summary>
    /// A view model for storing all sources for a specific scene
    /// </summary>
    public class SourceDeckViewModel : BaseListDeckViewModel<SourceViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;

        public SceneViewModel Scene { get; }

        public SourceDeckViewModel(SceneViewModel scene) {
            this.Scene = scene;
        }

        static SourceDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        public override async Task AddAction() {
            await IoC.MessageDialogs.ShowMessageAsync("Coming soon", "This feature is coming soon!");
        }

        public override async Task RemoveItemAction(SourceViewModel item) {
            if (!this.items.Contains(item)) {
                return;
            }

            string result = await ConfirmRemoveDialog.ShowAsync("Remove input/source?", $"Are you sure you want to remove {(string.IsNullOrEmpty(item.ReadableName) ? "this source" : item.ReadableName)}?");
            if (result == "yes") {
                await base.RemoveItemAction(item);
            }
        }
    }
}