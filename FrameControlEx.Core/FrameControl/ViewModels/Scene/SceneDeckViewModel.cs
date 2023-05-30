using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.Views.Dialogs.Message;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.FrameControl.Scene {
    public class SceneDeckViewModel : BaseListDeckViewModel<SceneViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;
        private readonly bool isModelLoaded;

        public FrameControlViewModel FrameControl { get; }

        public SceneDeckModel Model { get; }
        public InputValidator RenameValidator { get; }

        static SceneDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        public SceneDeckViewModel(FrameControlViewModel frameControl) {
            this.FrameControl = frameControl;
            this.Model = frameControl.Model.SceneDeck;

            foreach (SceneModel scene in this.Model.Scenes) {
                this.Add(new SceneViewModel(this, scene));
            }

            this.isModelLoaded = true;

            if (this.Items.Count > 0) {
                this.SelectedItems = new List<SceneViewModel>() {
                    this.Items[this.Items.Count - 1]
                };
            }

            this.RenameValidator = InputValidator.FromFunc((input) => {
                if (string.IsNullOrWhiteSpace(input)) {
                    return "Input cannot be an empty string";
                }
                else if (string.IsNullOrWhiteSpace(input)) {
                    return "Input cannot consist of only whitespaces";
                }
                else if (this.Items.Any(x => x.DisplayName == input)) {
                    return "Scene already exists";
                }
                else {
                    return null;
                }
            });
        }

        protected override void OnItemAdded(SceneViewModel item) {
            if (this.isModelLoaded)
                this.Model.Scenes.Add(item.Model);
        }

        protected override void OnRemovingItem(SceneViewModel item) {
            this.Model.Scenes.Remove(item.Model);
        }

        protected override void OnClearing() {
            this.Model.Scenes.Clear();
        }

        protected override void OnPrimarySelectionChanged(SceneViewModel oldValue, SceneViewModel newValue) {
            base.OnPrimarySelectionChanged(oldValue, newValue);
            if (oldValue != null)
                oldValue.IsActive = false;
            if (newValue != null)
                newValue.IsActive = true;
        }

        public SceneViewModel CreateScene(string name, bool select = false) {
            SceneModel model = this.Model.CreateScene(name);
            SceneViewModel scene = new SceneViewModel(this, model);
            this.Add(scene);

            if (this.Items.Count == 1 || select) {
                this.SelectedItems = new List<SceneViewModel>() {scene};
            }

            return scene;
        }

        public override async Task AddActionAsync() {
            string name = await IoC.UserInput.ShowSingleInputDialogAsync("Create scene", "Input a name for the scene:", "my scene", Validators.ForNonEmptyString("Name cannot be empty"));
            if (!string.IsNullOrEmpty(name)) {
                this.CreateScene(name);
            }
        }

        public override async Task RemoveItemsAction(IList<SceneViewModel> list) {
            if (list.Count < 1) {
                return;
            }

            string result = await ConfirmRemoveDialog.ShowAsync($"Remove scene{(list.Count == 1 ? "" : "s")}?", $"Are you sure you want to remove {(list.Count == 1 && !string.IsNullOrEmpty(list[0].DisplayName) ? list[0].DisplayName : list.Count.ToString())}?");
            if (result == "yes") {
                await base.RemoveItemsAction(list);
            }
        }
    }
}