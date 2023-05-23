using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene.Sources;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.MainView.Scene {
    /// <summary>
    /// A view model for storing all sources for a specific scene
    /// </summary>
    public class SourceDeckViewModel : BaseListDeckViewModel<SourceViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;

        public SceneViewModel Scene { get; }

        public int CountEnabled => this.Items.Count(x => x.IsEnabled);

        public int CountDisabled => this.Items.Count(x => !x.IsEnabled);

        public AsyncRelayCommand AddImageCommand { get; }
        public AsyncRelayCommand AddLoopbackInput { get; }

        public RelayCommand EnableAllCommand { get; }
        public RelayCommand DisableAllCommand { get; }
        public RelayCommand ToggleEnabledAllCommand { get; }

        public event VisualDeckInvalidatedEventHandler OnVisualInvalidated;

        public SourceDeckViewModel(SceneViewModel scene) {
            this.Scene = scene;
            this.AddImageCommand = new AsyncRelayCommand(this.AddImageAction);
            this.AddLoopbackInput = new AsyncRelayCommand(this.AddLoopbackInputAction);
            this.EnableAllCommand = new RelayCommand(() => this.Items.ForEach(x => x.IsEnabled = true), () => this.Items.Any(x => !x.IsEnabled));
            this.DisableAllCommand = new RelayCommand(() => this.Items.ForEach(x => x.IsEnabled = false), () => this.Items.Any(x => x.IsEnabled));
            this.ToggleEnabledAllCommand = new RelayCommand(() => this.Items.ForEach(x => x.IsEnabled = !x.IsEnabled));
        }

        static SourceDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        public void OnIsEnabledChanged(SourceViewModel source) {
            this.RaisePropertyChanged(nameof(this.CountEnabled));
            this.RaisePropertyChanged(nameof(this.CountDisabled));
            this.EnableAllCommand.RaiseCanExecuteChanged();
            this.DisableAllCommand.RaiseCanExecuteChanged();
        }

        private async Task AddImageAction() {
            ImageSourceViewModel source = new ImageSourceViewModel {
                ReadableName = $"Image {this.Items.Count + 1}"
            };

            this.Add(source);
            this.InvalidateVisual();
        }

        private async Task AddLoopbackInputAction() {
            LoopbackSourceViewModel source = new LoopbackSourceViewModel {
                ReadableName = $"SIInput {this.Items.Count + 1}"
            };

            this.Add(source);
            this.InvalidateVisual();
        }

        public override async Task AddActionAsync() {
            await IoC.MessageDialogs.ShowMessageAsync("Wot", "Right click the '+' button and select an item to add");
        }

        public override async Task RemoveItemAction(SourceViewModel item) {
            if (!this.Contains(item)) {
                return;
            }

            string result = await ConfirmRemoveDialog.ShowAsync("Remove input/source?", $"Are you sure you want to remove {(string.IsNullOrEmpty(item.ReadableName) ? "this source" : item.ReadableName)}?");
            if (result == "yes") {
                await base.RemoveItemAction(item);
                this.InvalidateVisual();
            }
        }

        // was originally going to make rendering event based, but that would just add extra overhead
        // because there may be multiple sources whose frames aren't necessarily event base-able (e.g.
        // capturing desktop; you do that event 60th of a sec or whatever the refresh rate is)

        // so instead, the main window just has a dispatcher callback running as quickly as possible
        // constantly rendering the output frame. It does use a bit of extra GPU... but that's probably alright

        public void InvalidateVisual() {
            this.OnVisualInvalidated?.Invoke();
        }

        protected override void EnsureItem(SourceViewModel item, bool valid) {
            base.EnsureItem(item, valid);
            item.Deck = valid ? this : null;
        }

        protected override void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            base.OnItemCollectionChanged(sender, e);
            this.RaisePropertyChanged(nameof(this.CountEnabled));
            this.RaisePropertyChanged(nameof(this.CountDisabled));
        }
    }
}