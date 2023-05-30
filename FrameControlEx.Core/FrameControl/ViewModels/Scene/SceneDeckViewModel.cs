using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.RBC;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs;
using FrameControlEx.Core.Views.Dialogs.Message;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene {
    public class SceneDeckViewModel : BaseListDeckViewModel<SceneViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;
        private bool canBaseAddToModel;

        public FrameControlViewModel FrameControl { get; }

        public SceneDeckModel Model { get; }

        public InputValidator RenameValidator { get; }

        public AsyncRelayCommand LoadLayoutCommand { get; }

        public AsyncRelayCommand SaveLayoutCommand { get; }

        public AsyncRelayCommand ClearLayoutCommand { get; }

        private string loadedLayoutName;
        public string LoadedLayoutName {
            get => this.loadedLayoutName;
            set => this.RaisePropertyChanged(ref this.loadedLayoutName, value);
        }

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

            this.canBaseAddToModel = true;

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

            this.ClearLayoutCommand = new AsyncRelayCommand(this.ClearLayoutAction);
            this.LoadLayoutCommand = new AsyncRelayCommand(this.LoadLayoutAction);
            this.SaveLayoutCommand = new AsyncRelayCommand(this.SaveLayoutAction);
        }

        public async Task ClearLayoutAction() {
            using (ExceptionStack stack = new ExceptionStack(null, false)) {
                this.DisposeItemsAndClear(stack);
                if (stack.TryGetException(out Exception exception)) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception disposing", "An exception occurred while disposing the deck", exception.GetToString());
                }
            }
        }

        public async Task LoadLayoutAction() {
            DialogResult<string[]> result = IoC.FilePicker.ShowFilePickerDialog(Filters.FrameControlSceneDeckType, null, "Select a layout to load", false);
            if (result.IsSuccess && result.Value.Length == 1 && !string.IsNullOrEmpty(result.Value[0])) {
                RBEBase item;
                try {
                    item = RBEUtils.ReadFromFile(result.Value[0]);
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception reading data", "An exception occurred while reading data from the file", e.GetToString());
                    return;
                }

                if (!(item is RBEDictionary dictionary)) {
                    await IoC.MessageDialogs.ShowMessageAsync("Invalid data", "The file did not contain a valid scene deck");
                    return;
                }

                this.Clear();
                this.Model.LoadLayout(dictionary);
                try {
                    this.canBaseAddToModel = false;
                    foreach (SceneModel scene in this.Model.Scenes) {
                        this.Add(new SceneViewModel(this, scene));
                    }
                }
                finally {
                    this.canBaseAddToModel = true;
                }
            }
        }

        public async Task SaveLayoutAction() {
            DialogResult<string> result = IoC.FilePicker.ShowSaveFileDialog(Filters.FrameControlSceneDeckType, null, "Select a layout to load");
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Value)) {
                RBEDictionary dictionary = new RBEDictionary();
                try {
                    this.Model.SaveLayout(dictionary);
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception saving layout", "An exception occurred while saving the scene deck layout", e.GetToString());
                    return;
                }

                try {
                    RBEUtils.WriteToFile(dictionary, result.Value);
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception writing data", "An exception occurred while writing the data to the file", e.GetToString());
                }
            }
        }

        protected override void OnItemAdded(SceneViewModel item) {
            if (this.canBaseAddToModel)
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