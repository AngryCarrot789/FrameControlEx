using System.Collections.Generic;
using System.Threading.Tasks;
using FrameControlEx.Core.Views.Dialogs.Message;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.FrameControl.Scene {
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

        protected override void OnPrimarySelectionChanged(SceneViewModel oldValue, SceneViewModel newValue) {
            base.OnPrimarySelectionChanged(oldValue, newValue);
            if (oldValue != null)
                oldValue.IsActive = false;
            if (newValue != null)
                newValue.IsActive = true;
        }

        public void AddNewScene(string name) {
            this.Add(new SceneViewModel(this) {
                ReadableName = name
            });

            this.SelectedItems = new List<SceneViewModel>() {
                this.Items[this.Items.Count - 1]
            };
        }

        public override async Task AddActionAsync() {
            string name = await IoC.UserInput.ShowSingleInputDialogAsync("Create scene", "Input a name for the scene:", "my scene", Validators.ForNonEmptyString("Name cannot be empty"));
            if (!string.IsNullOrEmpty(name)) {
                this.AddNewScene(name);
            }
        }

        public override async Task RemoveItemsAction(IList<SceneViewModel> list) {
            if (list.Count < 1) {
                return;
            }

            string result = await ConfirmRemoveDialog.ShowAsync($"Remove scene{(list.Count == 1 ? "" : "s")}?", $"Are you sure you want to remove {(list.Count == 1 && !string.IsNullOrEmpty(list[0].ReadableName) ? list[0].ReadableName : list.Count.ToString())}?");
            if (result == "yes") {
                await base.RemoveItemsAction(list);
            }
        }
    }
}