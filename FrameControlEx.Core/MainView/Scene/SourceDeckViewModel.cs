using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene.Sources;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.MainView.Scene {
    /// <summary>
    /// A view model for storing all sources for a specific scene
    /// </summary>
    public class SourceDeckViewModel : BaseListDeckViewModel<SourceViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;

        public SceneViewModel Scene { get; }

        public AsyncRelayCommand AddImageCommand { get; }
        public AsyncRelayCommand AddSameInstanceInputCommand { get; }

        static SourceDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        public SourceDeckViewModel(SceneViewModel scene) {
            this.Scene = scene;
            this.AddImageCommand = new AsyncRelayCommand(this.AddImageActionAsync);
            this.AddSameInstanceInputCommand = new AsyncRelayCommand(this.AddSameInstanceInputAction);
        }

        private async Task AddImageActionAsync() {
            ImageSourceViewModel source = new ImageSourceViewModel(this) {
                ReadableName = $"Image {this.items.Count + 1}"
            };

            this.items.Add(source);
            this.OnVisualInvalidated();
        }

        private async Task AddSameInstanceInputAction() {
            SameInstanceInputViewModel source = new SameInstanceInputViewModel(this) {
                ReadableName = $"SIInput {this.items.Count + 1}"
            };

            this.items.Add(source);
            this.OnVisualInvalidated();
        }

        public override async Task AddActionAsync() {
            await IoC.MessageDialogs.ShowMessageAsync("Wot", "Right click the '+' button and select an item to add");
        }

        public override async Task RemoveItemAction(SourceViewModel item) {
            if (!this.items.Contains(item)) {
                return;
            }

            string result = await ConfirmRemoveDialog.ShowAsync("Remove input/source?", $"Are you sure you want to remove {(string.IsNullOrEmpty(item.ReadableName) ? "this source" : item.ReadableName)}?");
            if (result == "yes") {
                await base.RemoveItemAction(item);
                this.OnVisualInvalidated();
            }
        }

        public void OnVisualSourceInvalidated(VisualSourceViewModel source) {
            this.OnVisualInvalidated();
        }

        public void OnVisualInvalidated() {
            this.Scene.Deck.FrameControl.PreviewRenderSurface.Render();
        }
    }
}