using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene.Outputs;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.MainView.Scene {
    /// <summary>
    /// A view model for storing all outputs for a specific scene
    /// </summary>
    public class OutputDeckViewModel : BaseListDeckViewModel<OutputViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;

        public FrameControlViewModel FrameControl { get; }

        // not sure how else to bypass dialogs :/
        public bool InternalBypassDialog { get; set; }

        public AsyncRelayCommand AddBufferedOutputCommand { get; }

        public OutputDeckViewModel(FrameControlViewModel frameControl) {
            this.FrameControl = frameControl;
            this.AddBufferedOutputCommand = new AsyncRelayCommand(this.AddBufferedOutputAction);
        }

        static OutputDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        private async Task AddBufferedOutputAction() {
            BasicBufferOutputViewModel source = new BasicBufferOutputViewModel() {
                ReadableName = $"SIOutput {this.Items.Count + 1}"
            };

            this.Add(source);
        }

        public override async Task AddActionAsync() {
            await IoC.MessageDialogs.ShowMessageAsync("Coming soon", "This feature is coming soon!");
        }

        public override async Task RemoveItemAction(OutputViewModel item) {
            if (!this.Contains(item)) {
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

        protected override void EnsureItem(OutputViewModel item, bool valid) {
            base.EnsureItem(item, valid);
            item.Deck = valid ? this : null;
        }
    }
}