using System;
using FrameControlEx.Core.FrameControl.ViewModels.Scene;
using FrameControlEx.Core.Views.Dialogs;

namespace FrameControlEx.FrameControl.Views {
    public class SceneSelectorViewModel : BaseConfirmableDialogViewModel {
        public SceneDeckViewModel TargetDeck { get; }

        private SceneViewModel selectedItem;
        public SceneViewModel SelectedItem {
            get => this.selectedItem;
            set {
                this.RaisePropertyChanged(ref this.selectedItem, value);
                this.ConfirmCommand.IsEnabled = value != null;
            }
        }

        private Predicate<SceneViewModel> isItemEnabled;
        public Predicate<SceneViewModel> IsItemEnabled {
            get => this.isItemEnabled;
            set => this.RaisePropertyChanged(ref this.isItemEnabled, value);
        }

        public SceneSelectorViewModel(SceneDeckViewModel deck, Predicate<SceneViewModel> filter) {
            this.TargetDeck = deck ?? throw new ArgumentNullException(nameof(deck));
            this.IsItemEnabled = filter;
            this.ConfirmCommand.IsEnabled = false;
        }
    }
}
