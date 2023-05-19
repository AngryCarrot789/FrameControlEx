using System.Threading.Tasks;
using FrameControlEx.Core.Views.Dialogs.Message;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.MainView.Scene {
    public class SceneDeckViewModel : BaseListDeckViewModel<SceneViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;

        public FrameControlViewModel FrameControl { get; }

        static SceneDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        public SceneDeckViewModel(FrameControlViewModel frameControl) {
            this.FrameControl = frameControl;
        }

        public void AddNewScene(string name) {
            this.items.Add(new SceneViewModel(this) {
                ReadableName = name
            });

            this.SelectedItem = this.items[this.items.Count - 1];
        }

        public override async Task AddActionAsync() {
            string name = await IoC.UserInput.ShowSingleInputDialogAsync("Create scene", "Input a name for the scene:", "my scene", Validators.ForNonEmptyString("Name cannot be empty"));
            if (!string.IsNullOrEmpty(name)) {
                this.AddNewScene(name);
            }
        }

        public override async Task RemoveItemAction(SceneViewModel item) {
            if (!this.items.Contains(item)) {
                return;
            }

            string result = await ConfirmRemoveDialog.ShowAsync("Remove scene?", $"Are you sure you want to remove {(string.IsNullOrEmpty(item.ReadableName) ? "this scene" : item.ReadableName)}?");
            if (result == "yes") {
                await base.RemoveItemAction(item);
            }
        }
    }
}